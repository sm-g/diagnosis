using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Controls;
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
        private List<IcdBlock> blocksBeforeEdit;

        private Speciality spec;
        private PopupSearchViewModel<IcdBlock> _diagnosisSearch2;

        public SpecialityEditorViewModel(Speciality spec)
        {
            Contract.Requires(spec != null);
            this.spec = spec;
            spec.BlocksChanged += spec_BlocksChanged;
            blocksBeforeEdit = new List<IcdBlock>(spec.IcdBlocks);

            (spec as IEditableObject).BeginEdit();

            Chapters = new ObservableCollection<DiagnosisViewModel>();
            Chapters2 = new ObservableCollection<DiagnosisViewModel>();

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
            DiagnosisSearch2.Filter.Clear();

            Title = "Редактор специальности";
            HelpTopic = "editspeciality";
            WithHelpButton = false;
        }

        /// <summary>
        /// Корневые элементы дерева.
        /// </summary>
        public ObservableCollection<DiagnosisViewModel> Chapters { get; private set; }

        /// <summary>
        /// Корневые элементы дерева.
        /// </summary>
        public ObservableCollection<DiagnosisViewModel> Chapters2 { get; private set; }

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
                    OnPropertyChanged(() => DiagnosisSearch);
                }
            }
        }

        public PopupSearchViewModel<IcdBlock> DiagnosisSearch2
        {
            get
            {
                return _diagnosisSearch2;
            }
            private set
            {
                if (_diagnosisSearch2 != value)
                {
                    _diagnosisSearch2 = value;
                    OnPropertyChanged(() => DiagnosisSearch2);
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
                        .SelectMany(ch => ch.IsSelected ? ch.Children : ch.Children.Where(b => b.IsSelected)) // все выделенные блоки
                        .ToList();
                    selecetedBlocks.ForEach(vm =>
                        spec.AddBlock(vm.Icd as IcdBlock));
                },
                () => Chapters != null && Chapters.Any(ch => ch.IsSelected || ch.Children.Any(b => b.IsSelected)));
            }
        }

        public RelayCommand RemoveBlocksCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //Speciality.SelectedBlocks.ToList().ForAll(vm =>
                    //    spec.RemoveBlock(vm.Icd as IcdBlock));
                    var selecetedBlocks = Chapters2
                       .SelectMany(ch => ch.IsSelected ? ch.Children : ch.Children.Where(b => b.IsSelected)) // все выделенные блоки
                       .ToList();
                    selecetedBlocks.ForEach(vm =>
                        spec.RemoveBlock(vm.Icd as IcdBlock));
                },
                () => Chapters2 != null && Chapters2.Any(ch => ch.IsSelected || ch.Children.Any(b => b.IsSelected)));
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
                spec.BlocksChanged -= spec_BlocksChanged;
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
        private static void MakeVms(ObservableCollection<DiagnosisViewModel> chapters, IEnumerable<IcdBlock> results)
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
                var chVm = chapters.Where(i => i.Icd as IcdChapter == ch).FirstOrDefault();
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

            chapters.SyncWith(chVms, compareKey);
        }

        private void CreateDiagnosisSearch()
        {
            var query = DiagnosisQuery.BlockStartingWith(Session);
            DiagnosisSearch = new PopupSearchViewModel<IcdBlock>(query);
            DiagnosisSearch.Filter.Filtered += (s, e) =>
            {
                inFiltered = true;
                MakeVms(Chapters, DiagnosisSearch.Filter.Results);
                inFiltered = false;
            };
            DiagnosisSearch.IsResultsVisible = true;

            Func<string, IEnumerable<IcdBlock>> specBlocksQuery = (s) =>
            {
                return spec.IcdBlocks.Where(x =>
                    x.Code.StartsWith(s, StringComparison.OrdinalIgnoreCase) ||
                    x.Title.StartsWith(s, StringComparison.OrdinalIgnoreCase));
            };
            DiagnosisSearch2 = new PopupSearchViewModel<IcdBlock>(specBlocksQuery);
            DiagnosisSearch2.Filter.Filtered += (s, e) =>
            {
                inFiltered = true;
                MakeVms(Chapters2, DiagnosisSearch2.Filter.Results);
                inFiltered = false;
            };
        }

        private void spec_BlocksChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MakeVms(Chapters2, spec.IcdBlocks);
        }
    }
}