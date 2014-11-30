using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using log4net;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class IcdSelectorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(IcdSelectorViewModel));

        private ObservableCollection<DiagnosisViewModel> _chapters;
        private IcdDisease _selected;
        private PopupSearchViewModel<IcdDisease> _diagnosisSearch;

        public IcdSelectorViewModel(IcdDisease initial = null)
        {
            _chapters = new ObservableCollection<DiagnosisViewModel>();
            SelectedIcd = initial;
            CreateDiagnosisSearch();
            UpdateDiagnosisQueryCode(initial, true);

            Title = "Выбор диагноза МКБ";
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
                return DiagnosisSearch.SelectedItem != null;
            }
        }

        private void MakeVms(ObservableCollection<IcdDisease> results)
        {
            var dict = (from d in results
                        group d by d.IcdBlock.IcdChapter into gr
                        select new
                        {
                            Ch = gr.Key,
                            Blocks = (from be in gr
                                      group be by be.IcdBlock into grr
                                      select new
                                      {
                                          Block = grr.Key,
                                          Diseases = grr.AsEnumerable()
                                      }).ToDictionary(x => x.Block, x => x.Diseases)
                        }).ToDictionary(x => x.Ch, x => x.Blocks);

            var q = from d in results
                    group d by new { d.IcdBlock, d.IcdBlock.IcdChapter } into gr
                    select new
                    {
                        Ch = gr.Key.IcdChapter,
                        B = gr.Key.IcdBlock,
                        Ds = gr.Select(x => x)
                    };

            //var bg = from d in results
            //         group d by d.IcdBlock into gr
            //         group gr by gr.Key.IcdChapter into ggr
            //         select new {
            //             Ch = ggr.Key,
            //             Blocked = new {
            //                 Block =
            //             }
            //         }

            //   var ch = bg.SelectMany(x => x);

            var blocks = results.Select(d => d.IcdBlock).Distinct();
            var chs = blocks.Select(b => b.IcdChapter).Distinct();

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

                            dVm.IsExpanded = true;
                            return dVm;
                        });

                        bVm.IsExpanded = true;
                        bVm.Children.SyncWith(dVms);
                        return bVm;
                    });

                    chVm.IsExpanded = true;
                    chVm.Children.SyncWith(bVms);
                    return chVm;
                });

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
                }
            };
            DiagnosisSearch.Filter.Filtered += (s, e) =>
            {
                MakeVms(DiagnosisSearch.Filter.Results);
            };
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