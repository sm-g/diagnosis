using Diagnosis.Core;
using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class AppointmentViewModel : ViewModelBase
    {
        internal readonly Appointment appointment;
        private HealthRecordManager hrManager;
        private ICollectionView healthRecordsView;
        private ShortHealthRecordViewModel _selectedHealthRecord;

        #region Model

        public Doctor Doctor
        {
            get { return appointment.Doctor; }
        }

        public DateTime DateTime
        {
            get { return appointment.DateAndTime; }
        }

        public ObservableCollection<ShortHealthRecordViewModel> HealthRecords
        {
            get
            {
                if (healthRecordsView == null)
                {
                    healthRecordsView = (CollectionView)CollectionViewSource.GetDefaultView(hrManager.HealthRecords);
                    PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
                    SortDescription sort1 = new SortDescription("Category", ListSortDirection.Ascending);
                    SortDescription sort2 = new SortDescription("SortingDate", ListSortDirection.Ascending);
                    healthRecordsView.GroupDescriptions.Add(groupDescription);
                    healthRecordsView.SortDescriptions.Add(sort1);
                    healthRecordsView.SortDescriptions.Add(sort2);
                }
                return hrManager.HealthRecords;
            }
        }

        #endregion Model

        public ShortHealthRecordViewModel SelectedHealthRecord
        {
            get
            {
                return _selectedHealthRecord;
            }
            set
            {
                _selectedHealthRecord = value;
                OnPropertyChanged(() => SelectedHealthRecord);
            }
        }

        public AppointmentViewModel(Appointment appointment)
        {
            Contract.Requires(appointment != null);
            this.appointment = appointment;

            hrManager = new HealthRecordManager(appointment, onHrVmPropChanged: (s, e) =>
            {
                if (e.PropertyName == "Category")
                {
                    healthRecordsView.Refresh();
                }
                else if (e.PropertyName == "IsChecked")
                {
                    OnPropertyChanged("CheckedHrCount");
                }
            });
        }

        public override string ToString()
        {
            return appointment.ToString();
        }

        internal void SelectHealthRecord(HealthRecord healthRecord)
        {
            SelectedHealthRecord = HealthRecords.FirstOrDefault(x => x.healthRecord == healthRecord);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    hrManager.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}