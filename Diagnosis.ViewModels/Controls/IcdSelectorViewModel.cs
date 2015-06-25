using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.ViewModels.Controls
{
    public class IcdSelectorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(IcdSelectorViewModel));
        private static int MinQueryToExpandBlock = 3;

        private AsyncObservableCollection<DiagnosisViewModel> _chapters;
        private IcdDisease _selected;
        private PopupSearchViewModel<IcdDisease> _diagnosisSearch;
        private bool inFiltered;
        private bool _icdTopLevelOnly;

        public IcdSelectorViewModel(IcdDisease initial = null, string query = null)
        {
            var doctor = AuthorityController.CurrentDoctor;
            _icdTopLevelOnly = doctor != null ? doctor.Settings.IcdTopLevelOnly : false;

            _chapters = new AsyncObservableCollection<DiagnosisViewModel>();
            CreateDiagnosisSearch();

            UpdateDiagnosisQueryCode(initial, true);

            if (query != null)
            {
                DiagnosisSearch.Filter.Query = query;
            }

            Title = "Выбор диагноза МКБ";
            HelpTopic = "icdselector";
            WithHelpButton = false;

            SelectedIcd = initial;
            DiagnosisSearch.Filter.IsFocused = true; // TODO фокус на список если выбранно
        }

        /// <summary>
        /// Create, edit
        /// </summary>
        /// <param name="initial"></param>
        public IcdSelectorViewModel(IcdDisease initial)
            : this(initial, null)
        {
        }

        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="query"></param>
        public IcdSelectorViewModel(string query)
            : this(null, query)
        {
        }

        public bool IcdTopLevelOnly
        {
            get
            {
                return _icdTopLevelOnly;
            }
            set
            {
                if (_icdTopLevelOnly != value)
                {
                    _icdTopLevelOnly = value;

                    IcdDisease toSelect = SelectedIcd;

                    MakeVms(DiagnosisSearch.Filter.Results);
                    if (value && toSelect != null)
                    {
                        // выбираем рубрику, если была подрубрика
                        if (SelectedIcd.IsSubdivision)
                            toSelect = DiagnosisSearch.Filter.Results.FirstOrDefault(x => x.Code == SelectedIcd.DivisionCode);
                    }
                    SelectedIcd = toSelect;

                    OnPropertyChanged(() => IcdTopLevelOnly);
                }
            }
        }

        public IcdDisease SelectedIcd
        {
            get
            {
                return _selected;
            }
            private set
            {
                if (_selected != value)
                {
                    _selected = value;
                    if (value != null)
                    {
                        // выбираем соответствующую VM
                        var vm = Chapters.First(x => x.Code == value.IcdBlock.IcdChapter.Code)
                            .Children.First(x => x.Code == value.IcdBlock.Code)
                            .Children.First(x => x.Code == value.Code);
                        vm.IsSelected = true;
                    }
                    OnPropertyChanged(() => SelectedIcd);
                }
            }
        }

        /// <summary>
        /// Корневые элементы дерева.
        /// </summary>
        public ObservableCollection<DiagnosisViewModel> Chapters { get { return _chapters; } }

        public PopupSearchViewModel<IcdDisease> DiagnosisSearch
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

        public override bool CanOk
        {
            get
            {
                return SelectedIcd != null;
            }
        }

        protected override void OnOk()
        {
            var doctor = AuthorityController.CurrentDoctor;
            if (doctor == null)
                return;

            doctor.Settings.IcdTopLevelOnly = IcdTopLevelOnly;
            Session.DoSave(doctor);
        }

        /// <summary>
        /// Обновляет VMs по найденным болезням
        /// </summary>
        /// <param name="results"></param>
        private void MakeVms(ObservableCollection<IcdDisease> results)
        {
            var doctor = AuthorityController.CurrentDoctor;

            Func<IcdDisease, bool> diseaseClause = d => true;
            if (IcdTopLevelOnly)
            {
                // без уточненных болезней
                diseaseClause = d => !d.IsSubdivision;
            }
            Func<IcdBlock, bool> blockClause = b => true;
            if (doctor != null && doctor.Speciality != null)
            {
                // блоки для специальности доктора
                blockClause = b => doctor.Speciality.IcdBlocks.Contains(b);
            }

            Dictionary<IcdChapter, Dictionary<IcdBlock, IEnumerable<IcdDisease>>>
                dict = (from d in results
                        group d by d.IcdBlock.IcdChapter into gr
                        let blocks = (from be in gr
                                      group be by be.IcdBlock into grr
                                      where blockClause(grr.Key)
                                      select new
                                      {
                                          Block = grr.Key,
                                          Diseases = grr.Where(diseaseClause)
                                      }).ToDictionary(x => x.Block, x => x.Diseases)
                        where blocks.Count > 0 // без пустых классов
                        select new
                        {
                            Chapter = gr.Key,
                            Blocks = blocks
                        }).ToDictionary(x => x.Chapter, x => x.Blocks);

            // для каждого класса, блока и болезни ищем существующую VM или создаем
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

                var bVms = dict[ch].Keys.Select(b =>
                {
                    var bVm = chVm.Children.Where(i => i.Icd as IcdBlock == b).FirstOrDefault();
                    if (bVm == null)
                    {
                        bVm = new DiagnosisViewModel(b);
                        bVm.PropertyChanged += (s, e) =>
                        {
                            // сохраняем выбор пользователя
                            if (e.PropertyName == "IsExpanded" && !inMaking)
                            {
                                bVm.UserExpaneded = bVm.IsExpanded;
                            }
                        };
                    }
                    var dVms = dict[ch][b].Select(d =>
                    {
                        var dVm = bVm.Children.Where(i => i.Icd as IcdDisease == d)
                            .FirstOrDefault() ?? new DiagnosisViewModel(d);

                        return dVm;
                    }).ToList();

                    // блоки остаются в состоянии, выбранном пользователем
                    bVm.IsExpanded = (bVm.UserExpaneded ?? false) ? true : // был раскрыт пользователем
                        (TypedEnough() && (bVm.UserExpaneded ?? true)); // или запрос достаточно точный и блок не был свернут

                    bVm.Children.SyncWith(dVms);
                    return bVm;
                }).ToList();

                // глава раскрыта, если не была свернута пользователем
                chVm.IsExpanded = chVm.UserExpaneded ?? true;

                chVm.Children.SyncWith(bVms);
                return chVm;
            }).ToList();
            inMaking = false;

            // всегда раскрыта глава и блок с выбранной болезнью (hieratchicalbase)

            // TODO длинный запрос — vm удаляются, сохранять UserExpaneded для каждой

            Chapters.SyncWith(chVms);
        }

        private bool TypedEnough()
        {
            // запрос не после выбора и длинный
            return inFiltered && DiagnosisSearch.Filter.AutoFiltered;
        }

        private void CreateDiagnosisSearch()
        {
            DiagnosisSearch = new PopupSearchViewModel<IcdDisease>(
                IcdQuery.StartingWith(Session)
                );
            DiagnosisSearch.Filter.AutoFilterMinQueryLength = MinQueryToExpandBlock;
            DiagnosisSearch.Filter.Cleared += (s, e) =>
            {
                SelectedIcd = null;
            };
            DiagnosisSearch.ResultItemSelected += (s, e) =>
            {
                var dvm = e.arg as DiagnosisViewModel;
                if (dvm != null)
                {
                    SelectedIcd = dvm.Icd as IcdDisease;
                    UpdateDiagnosisQueryCode(dvm.Icd as IcdDisease);
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

        private void UpdateDiagnosisQueryCode(IcdDisease d, bool updateResult = false)
        {
            if (DiagnosisSearch != null)
            {
                DiagnosisSearch.Filter.DoAutoFilter = updateResult;

                if (d != null)
                    DiagnosisSearch.Filter.Query = d.Code;
                else
                    DiagnosisSearch.Filter.Clear();

                DiagnosisSearch.Filter.DoAutoFilter = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DiagnosisSearch.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}