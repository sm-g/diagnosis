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
        public SearchResultViewModel(IEnumerable<HealthRecord> hrs)
        {
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
