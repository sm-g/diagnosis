using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class AppointmentViewModel : ViewModelBase
    {
        private Appointment appointment;

        private CourseViewModel courseVM;
        private DoctorViewModel _doctor;
        private HealthRecordViewModel _selectedHealthRecord;
        private ICommand _addHealthRecord;

        public DoctorViewModel Doctor
        {
            get
            {
                return _doctor;
            }
            set
            {
                if (_doctor != value)
                {
                    _doctor = value;
                    OnPropertyChanged(() => Doctor);
                    OnPropertyChanged(() => IsDoctorFromCourse);
                }
            }
        }

        public DateTime DateTime
        {
            get
            {
                return appointment.DateAndTime;
            }
        }

        public ObservableCollection<HealthRecordViewModel> HealthRecords { get; private set; }

        public ICollectionView HealthRecordsView { get; private set; }

        public bool IsDoctorFromCourse
        {
            get
            {
                return Doctor == courseVM.LeadDoctor;
            }
        }

        public HealthRecordViewModel SelectedHealthRecord
        {
            get
            {
                return _selectedHealthRecord;
            }
            set
            {
                if (_selectedHealthRecord != value)
                {
                    _selectedHealthRecord = value;
                    if (value != null)
                    {
                        _selectedHealthRecord.IsSelected = true;
                        // this.Send((int)EventID.HealthRecordSelected, new HealthRecordSelectedParams(_selectedHealthRecord).Params);
                    }

                    OnPropertyChanged(() => SelectedHealthRecord);
                }
            }
        }

        public ICommand AddHealthRecordCommand
        {
            get
            {
                return _addHealthRecord
                    ?? (_addHealthRecord = new RelayCommand(() =>
                        {
                            AddHealthRecord();
                        }));
            }
        }

        public void AddHealthRecord()
        {
            var hrVM = NewHealthRecord();

            SelectedHealthRecord = hrVM;
        }

        public AppointmentViewModel(Appointment appointment, CourseViewModel courseVM)
        {
            Contract.Requires(appointment != null);
            Contract.Requires(courseVM != null);

            this.appointment = appointment;
            this.courseVM = courseVM;

            Doctor = EntityManagers.DoctorsManager.GetByModel(appointment.Doctor);

            var hrVMs = appointment.HealthRecords.Select(hr => new HealthRecordViewModel(hr)).ToList();
            hrVMs.ForAll(hr => SubscribeHR(hr));
            HealthRecords = new ObservableCollection<HealthRecordViewModel>(hrVMs);

            SetupHealthRecordsView();
        }

        private void SetupHealthRecordsView()
        {
            HealthRecordsView = (CollectionView)CollectionViewSource.GetDefaultView(HealthRecords);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            SortDescription sort1 = new SortDescription("Category", ListSortDirection.Ascending);
            SortDescription sort2 = new SortDescription("SortingDate", ListSortDirection.Ascending);
            HealthRecordsView.GroupDescriptions.Add(groupDescription);
            HealthRecordsView.SortDescriptions.Add(sort1);
            HealthRecordsView.SortDescriptions.Add(sort2);
        }

        private HealthRecordViewModel NewHealthRecord()
        {
            var hr = appointment.AddHealthRecord();
            var hrVM = new HealthRecordViewModel(hr);
            SubscribeHR(hrVM);
            HealthRecords.Add(hrVM);
            return hrVM;
        }

        private void SubscribeHR(HealthRecordViewModel hr)
        {
            hr.Editable.ModelPropertyChanged += (s, e) =>
            {
                this.Send((int)EventID.HealthRecordChanged,
                    new HealthRecordChangedParams(e.viewModel as HealthRecordViewModel).Params);

                HealthRecords.Remove(hr);
                HealthRecords.Add(hr);
            };
        }

        public override string ToString()
        {
            return DateTime.ToShortDateString() + ' ' + Doctor;
        }
    }
}