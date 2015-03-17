using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Search;
using log4net;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class SpecialityEditorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SpecialityEditorViewModel));

        private PopupSearchViewModel<IcdBlock> _diagnosisSearch;
        private bool inFiltered;
        List<IcdBlock> blocksBeforeEdit;

        private Speciality spec;

        public SpecialityEditorViewModel(Speciality spec)
        {
            Contract.Requires(spec != null);
            this.spec = spec;
            blocksBeforeEdit = new List<IcdBlock>(spec.IcdBlocks);

            (spec as IEditableObject).BeginEdit();

            Chapters = new ObservableCollection<DiagnosisViewModel>();

            var specs = Session.Query<Speciality>()
                .ToList();
            Speciality = new SpecialityViewModel(spec);
            Speciality.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Title")
                {
                    TestExisting(Speciality, specs);
                }
            };
            TestExisting(Speciality, specs);

            CreateDiagnosisSearch();
            DiagnosisSearch.Filter.Clear();

            Title = "Редактор специальности";
            HelpTopic = "editspeciality";
            WithHelpButton = false;
        }

        /// <summary>
        /// Корневые элементы дерева.
        /// </summary>
        public ObservableCollection<DiagnosisViewModel> Chapters { get; private set; }

        public SpecialityViewModel Speciality { get; set; }

        public PopupSearchViewModel<IcdBlock> DiagnosisSearch
        {
            get
            {
                return _diagnosisSearch;
            }
            private set
            {
                if (_diagnosisSearch != value)
                {
                    _diagnosisSearch = value;
                    OnPropertyChanged("DiagnosisSearch");
                }
            }
        }

        public RelayCommand AddBlocksCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var selecetedBlocks = Chapters
                        .SelectMany(ch => ch.Children.Where(b => b.IsSelected))
                        .ToList();
                    selecetedBlocks.ForEach(vm =>
                        spec.AddBlock(vm.Icd as IcdBlock));
                },
                () => Chapters != null && Chapters.Any(ch => ch.Children.Any(b => b.IsSelected)));
            }
        }

        public RelayCommand RemoveBlocksCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Speciality.SelectedBlocks.ToList().ForAll(vm =>
                        spec.RemoveBlock(vm.Icd as IcdBlock));
                },
                () => Speciality != null && Speciality.SelectedBlocks.Count > 0);
            }
        }

        public override bool CanOk
        {
            get
            {
                return spec.IsValid() && !Speciality.HasExistingTitle;
            }
        }

        protected override void OnOk()
        {
            (spec as IEditableObject).EndEdit();

            new Saver(Session).Save(spec);
        }

        protected override void OnCancel()
        {
            // should be in EditableObjectHelper...
            foreach (var b in blocksBeforeEdit)
            {
                if (!spec.IcdBlocks.Contains(b))
                {
                    spec.AddBlock(b);
                }
            }
            foreach (var b in spec.IcdBlocks)
            {
                if (!blocksBeforeEdit.Contains(b))
                {
                    spec.RemoveBlock(b);
                }
            }

            (spec as IEditableObject).CancelEdit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Speciality.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Нельзя ввести специальность, которая уже есть.
        /// </summary>
        private void TestExisting(SpecialityViewModel vm, IEnumerable<Speciality> specs)
        {
            vm.HasExistingTitle = specs.Any(s => s.Title == spec.Title && s != spec);
        }

        /// <summary>
        /// Обновляет VMs по найденным болезням
        /// </summary>
        /// <param name="results"></param>
        private void MakeVms(ObservableCollection<IcdBlock> results)
        {
            Dictionary<IcdChapter, IEnumerable<IcdBlock>>
                dict = (from d in results
                        group d by d.IcdChapter into gr
                        select new
                        {
                            Chapter = gr.Key,
                            Blocks = gr.Select(x => x)
                        }).ToDictionary(x => x.Chapter, x => x.Blocks);

            // для каждого класса, блока ищем существующую VM или создаем
            // разворачиваем
            // синхронизируем с детьми уровня выше
            Func<DiagnosisViewModel, IIcdEntity> compareKey = (vm) => vm.Icd;
            var inMaking = true;
            var chVms = dict.Keys.Select(ch =>
            {
                var chVm = Chapters.Where(i => i.Icd as IcdChapter == ch).FirstOrDefault();
                if (chVm == null)
                {
                    chVm = new DiagnosisViewModel(ch);
                    chVm.PropertyChanged += (s, e) =>
                    {
                        // сохраняем выбор пользователя
                        if (e.PropertyName == "IsExpanded" && !inMaking)
                        {
                            chVm.UserExpaneded = chVm.IsExpanded;
                        }
                    };
                }

                var bVms = dict[ch].Select(b =>
                {
                    var bVm = chVm.Children.Where(i => i.Icd as IcdBlock == b).FirstOrDefault();
                    if (bVm == null)
                    {
                        bVm = new DiagnosisViewModel(b);
                    }

                    return bVm;
                }).ToList();

                // глава раскрыта, если не была свернута пользователем
                chVm.IsExpanded = chVm.UserExpaneded ?? true;

                chVm.Children.SyncWith(bVms, compareKey);
                return chVm;
            }).ToList();
            inMaking = false;

            Chapters.SyncWith(chVms, compareKey);
        }

        private void CreateDiagnosisSearch()
        {
            var query = DiagnosisQuery.BlockStartingWith(Session);
            DiagnosisSearch = new PopupSearchViewModel<IcdBlock>(query);

            DiagnosisSearch.Filter.Cleared += (s, e) =>
            {
            };
            DiagnosisSearch.ResultItemSelected += (s, e) =>
            {
                var dvm = e.arg as DiagnosisViewModel;
                if (dvm != null)
                {
                }
            };
            DiagnosisSearch.Filter.Filtered += (s, e) =>
            {
                inFiltered = true;
                MakeVms(DiagnosisSearch.Filter.Results);
                inFiltered = false;
            };
            DiagnosisSearch.IsResultsVisible = true;
        }
    }
}