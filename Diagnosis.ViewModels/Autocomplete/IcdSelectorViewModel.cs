using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.ViewModels.Autocomplete
{
    public class IcdSelectorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(IcdSelectorViewModel));

        private ObservableCollection<DiagnosisViewModel> _chapters;
        private IcdDisease _selected;
        private PopupSearchViewModel<IcdDisease> _diagnosisSearch;
        Doctor doctor;

        private IcdSelectorViewModel(IcdDisease initial = null, string query = null)
        {
            doctor = AuthorityController.CurrentDoctor;

            _chapters = new ObservableCollection<DiagnosisViewModel>();
            CreateDiagnosisSearch();
            SelectedIcd = initial;
            UpdateDiagnosisQueryCode(initial, true);

            if (query != null)
            {
                DiagnosisSearch.Filter.Query = query;
            }

            Title = "Выбор диагноза МКБ";


        }

        public IcdSelectorViewModel(IcdDisease initial = null)
            : this(initial, null)
        {
        }

        public IcdSelectorViewModel(string query)
            : this(null, query)
        {
        }

        public bool IcdTopLevelOnly
        {
            get
            {
                return doctor.Settings.IcdTopLevelOnly;
            }
            set
            {
                doctor.Settings.IcdTopLevelOnly = value;
                OnPropertyChanged(() => IcdTopLevelOnly);
            }
        }

        public IcdDisease SelectedIcd
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
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
                    OnPropertyChanged("DiagnosisSearch");
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
            Session.Save(doctor);
        }

        /// <summary>
        /// Обновляет VMs по найденным болезням
        /// </summary>
        /// <param name="results"></param>
        private void MakeVms(ObservableCollection<IcdDisease> results)
        {

            Func<IcdDisease, bool> diseaseClause = d => true;
            if (doctor.Settings.IcdTopLevelOnly)
            {
                // без уточненных болезней
                diseaseClause = d => d.Code.IndexOf('.') == -1;
            }
            Func<IcdBlock, bool> blockClause = b => true;
            if (doctor.Speciality != null)
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
            var chVms = dict.Keys.Select(ch =>
            {
                var chVm = Chapters.Where(i => i.Icd as IcdChapter == ch)
                    .FirstOrDefault() ?? new DiagnosisViewModel(ch);

                var bVms = dict[ch].Keys.Select(b =>
                {
                    var bVm = chVm.Children.Where(i => i.Icd as IcdBlock == b)
                        .FirstOrDefault() ?? new DiagnosisViewModel(b);

                    var dVms = dict[ch][b].Select(d =>
                    {
                        var dVm = bVm.Children.Where(i => i.Icd as IcdDisease == d)
                            .FirstOrDefault() ?? new DiagnosisViewModel(d);

                        return dVm;
                    }).ToList();

                    bVm.IsExpanded = bVm.IsExpanded || DiagnosisSearch.Filter.Query.Length > 2; // expand block if enough info

                    bVm.Children.SyncWith(dVms); // TODO сохранить порядок
                    //bVm.Children.Clear();
                    //bVm.Add(dVms);
                    return bVm;
                }).ToList();

                chVm.IsExpanded = true;
                chVm.Children.SyncWith(bVms);
                return chVm;
            }).ToList();

            Chapters.SyncWith(chVms);
        }

        private void CreateDiagnosisSearch()
        {
            DiagnosisSearch = new PopupSearchViewModel<IcdDisease>(
                DiagnosisQuery.StartingWith(Session)
                );

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
                MakeVms(DiagnosisSearch.Filter.Results);
            };
            DiagnosisSearch.IsResultsVisible = true;
        }

        private void UpdateDiagnosisQueryCode(IcdDisease d, bool updateResult = false)
        {
            if (DiagnosisSearch != null)
            {
                DiagnosisSearch.Filter.UpdateResultsOnQueryChanges = updateResult;

                if (d != null)
                    DiagnosisSearch.Filter.Query = d.Code;
                else
                    DiagnosisSearch.Filter.Query = "";

                DiagnosisSearch.Filter.UpdateResultsOnQueryChanges = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}