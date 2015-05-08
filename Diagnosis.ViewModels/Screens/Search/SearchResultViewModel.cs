using Diagnosis.Models;
using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Diagnosis.ViewModels.Search;
using EventAggregator;

namespace Diagnosis.ViewModels.Screens
{
    public class SearchResultViewModel : ViewModelBase
    {
        private readonly SearchOptions _options;
        private EventMessageHandler handler;
        public SearchResultViewModel(IEnumerable<HealthRecord> hrs, SearchOptions options)
        {
            _options = options;

            Patients = new ObservableCollection<HolderSearchResultViewModel>();

            HolderSearchResultViewModel.MakeFrom(hrs).ForAll(x =>
            {
                x.ForBranch(rvm => rvm.IsExpanded = true);
                Patients.Add(x);
            });

            Statistic = new Statistic(hrs);

            handler = this.Subscribe(Event.DeleteHolder, (e) =>
            {
                // убираем удаленного холдера из результатов
                var h = e.GetValue<IHrsHolder>(MessageKeys.Holder);
                var hsr = Patients.FindHolderKeeperOf(h);
                if (hsr != null)
                {
                    if (hsr.IsRoot)
                        Patients.Remove(hsr);
                    else
                        hsr.Remove();
                }

            });
        }

        public ObservableCollection<HolderSearchResultViewModel> Patients { get; private set; }

        public Statistic Statistic { get; private set; }

        /// <summary>
        /// Опции, которыми получен результат.
        /// </summary>
        public SearchOptions Options
        {
            get { return _options; }
        }
        public RelayCommand ExportCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    new Exporter().ExportToXlsx(Statistic);
                });
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    handler.Dispose();
                }

            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
