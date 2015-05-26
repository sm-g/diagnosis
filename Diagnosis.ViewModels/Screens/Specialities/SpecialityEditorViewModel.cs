using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Controls;
using log4net;
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
        private ExistanceTester<Models.Speciality> tester;

        public SpecialityEditorViewModel(Speciality spec)
        {
            Contract.Requires(spec != null);
            this.spec = spec;
            spec.BlocksChanged += spec_BlocksChanged;
            spec.VocsChanged += spec_VocsChanged;
            blocksBeforeEdit = new List<IcdBlock>(spec.IcdBlocks);

            (spec as IEditableObject).BeginEdit();

            Chapters = new AsyncObservableCollection<DiagnosisViewModel>();
            SpecChapters = new AsyncObservableCollection<DiagnosisViewModel>();
            AllVocs = new ObservableCollection<VocabularyViewModel>();
            SpecVocs = new ObservableCollection<VocabularyViewModel>();

            Speciality = new SpecialityViewModel(spec);
            tester = new ExistanceTester<Speciality>(spec, Speciality, Session);
            tester.Test();

            CreateDiagnosisSearch();
            DiagnosisSearch.Filter.Clear();
            SpecDiagnosisSearch.Filter.Clear();

            VocabularyQuery.NonCustom(Session)()
                .ForAll(x =>
                {
                    if (x.Specialities.Contains(spec))
                        SpecVocs.Add(new VocabularyViewModel(x));
                    else
                        AllVocs.Add(new VocabularyViewModel(x));
                });

            Title = "Редактор специальности";
            HelpTopic = "editspeciality";
            WithHelpButton = false;
        }

        public SpecialityViewModel Speciality { get; set; }

        public ObservableCollection<VocabularyViewModel> SpecVocs { get; private set; }

        public ObservableCollection<VocabularyViewModel> AllVocs { get; private set; }

        /// <summary>
        /// Корневые элементы дерева.
        /// </summary>
        public ObservableCollection<DiagnosisViewModel> Chapters { get; private set; }

        /// <summary>
        /// Корневые элементы дерева.
        /// </summary>
        public ObservableCollection<DiagnosisViewModel> SpecChapters { get; private set; }

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

        public PopupSearchViewModel<IcdBlock> SpecDiagnosisSearch
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
                    OnPropertyChanged(() => SpecDiagnosisSearch);
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
                    var selecetedBlocks = SpecChapters
                       .SelectMany(ch => ch.IsSelected ? ch.Children : ch.Children.Where(b => b.IsSelected)) // все выделенные блоки
                       .ToList();
                    selecetedBlocks.ForEach(vm =>
                        spec.RemoveBlock(vm.Icd as IcdBlock));
                },
                () => SpecChapters != null && SpecChapters.Any(ch => ch.IsSelected || ch.Children.Any(b => b.IsSelected)));
            }
        }

        public RelayCommand AddVocsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var selected = AllVocs.Where(x => x.IsSelected).ToList();
                    selected.ForAll(vm =>
                    {
                        spec.AddVoc(vm.voc);
                        AllVocs.Remove(vm);
                    });
                },
                () => AllVocs != null && AllVocs.Any(v => v.IsSelected));
            }
        }

        public RelayCommand RemoveVocsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var selected = SpecVocs.Where(x => x.IsSelected).ToList();
                    selected.ForAll(vm =>
                    {
                        spec.RemoveVoc(vm.voc);
                        AllVocs.AddSorted(vm, x => x.Title);
                    });
                },
                () => SpecVocs != null && SpecVocs.Any(v => v.IsSelected));
            }
        }

        public override bool CanOk
        {
            get
            {
                return spec.IsValid() && !Speciality.HasExistingValue;
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
                spec.BlocksChanged -= spec_BlocksChanged;
                spec.VocsChanged -= spec_VocsChanged;

                tester.Dispose();
                Speciality.Dispose();
                SpecDiagnosisSearch.Dispose();
                DiagnosisSearch.Dispose();
            }
            base.Dispose(disposing);
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
            var query = IcdQuery.BlockStartingWith(Session);
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
            SpecDiagnosisSearch = new PopupSearchViewModel<IcdBlock>(specBlocksQuery);
            SpecDiagnosisSearch.Filter.Filtered += (s, e) =>
            {
                inFiltered = true;
                MakeVms(SpecChapters, SpecDiagnosisSearch.Filter.Results);
                inFiltered = false;
            };
        }

        private void spec_BlocksChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MakeVms(SpecChapters, spec.IcdBlocks);
        }

        private void spec_VocsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var vms = spec.Vocabularies.Select(v => SpecVocs
               .Where(vm => vm.voc == v)
               .FirstOrDefault() ?? new VocabularyViewModel(v));
            SpecVocs.SyncWith(vms, x => x.Title);
        }
    }
}