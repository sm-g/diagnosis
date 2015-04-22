using Diagnosis.Models;
using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Search
{
    public class SearchResultViewModel : ViewModelBase
    {
        private HrSearchOptions _options;
        public SearchResultViewModel(IEnumerable<HealthRecord> hrs, HrSearchOptions options)
        {
            Options = options;

            Patients = new ObservableCollection<HrHolderSearchResultViewModel>();

            HrHolderSearchResultViewModel.MakeFrom(hrs).ForAll(x =>
            {
                x.ForBranch(rvm => rvm.IsExpanded = true);
                Patients.Add(x);
            });

            Statistic = new Statistic(hrs);
        }

        public ObservableCollection<HrHolderSearchResultViewModel> Patients { get; private set; }

        public Statistic Statistic { get; private set; }

        /// <summary>
        /// Опции последнго поиска.
        /// </summary>
        public HrSearchOptions Options
        {
            get { return _options; }
            private set
            {
                _options = value;
                OnPropertyChanged("Options");
            }
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
    }
}
