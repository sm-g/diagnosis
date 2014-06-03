﻿using Diagnosis.App.Messaging;
using Diagnosis.Core;
using Diagnosis.Data;
using Diagnosis.Models;
using EventAggregator;
using NHibernate;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class AppointmentViewModel : ViewModelBase, IEditableNesting
    {
        internal readonly Appointment appointment;

        private CourseViewModel courseVM;
        private DoctorViewModel _doctor;
        private HealthRecordViewModel _selectedHealthRecord;

        private ICommand _addHealthRecord;
        private ICommand _deleteHealthRecords;
        private int _checkedHealthRecords;

        private bool movingToViewGroup;

        #region IEditableNesting

        public Editable Editable { get; private set; }

        /// <summary>
        /// Встреча пустая, если пусты все записи в ней или их нет.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return HealthRecords.All(hr => hr.IsEmpty);
            }
        }

        #endregion IEditableNesting

        #region Model

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

        #endregion Model

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
                    if (value != null)
                    {
                        value.IsSelected = true;
                        this.Send((int)EventID.HealthRecordSelected, new HealthRecordSelectedParams(value).Params);
                    }
                    else
                    {
                    }
                    _selectedHealthRecord = value;

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

        public ICommand DeleteHealthRecordsCommand
        {
            get
            {
                return _deleteHealthRecords
                    ?? (_deleteHealthRecords = new RelayCommand(() =>
                    {
                        DeleteCheckedHealthRecords();
                    }, () => CheckedHealthRecords > 0 || SelectedHealthRecord != null));
            }
        }

        public int CheckedHealthRecords
        {
            get
            {
                return _checkedHealthRecords;
            }
            set
            {
                if (_checkedHealthRecords != value)
                {
                    _checkedHealthRecords = value;
                    OnPropertyChanged(() => CheckedHealthRecords);
                }
            }
        }

        public AppointmentViewModel(Appointment appointment, CourseViewModel courseVM, bool firstInCourse = false)
        {
            Contract.Requires(appointment != null);
            Contract.Requires(courseVM != null);

            this.appointment = appointment;
            this.courseVM = courseVM;

            Editable = new Editable(this, dirtImmunity: true, switchedOn: true);
            if (firstInCourse)
            {
                Editable.CanBeDeleted = false;
            }

            Doctor = EntityManagers.DoctorsManager.GetByModel(appointment.Doctor);
            SetupHealthRecords(true);

            Editable.CanBeDirty = true;

            Subscribe();

            SetupHealthRecordsView();
        }

        private void SetupHealthRecords(bool withFirstHr)
        {
            var hrVMs = appointment.HealthRecords.Select(hr => new HealthRecordViewModel(hr)).ToList();
            hrVMs.ForAll(hr => SubscribeHR(hr));
            HealthRecords = new ObservableCollection<HealthRecordViewModel>(hrVMs);

            if (withFirstHr && HealthRecords.Count == 0)
            {
                AddHealthRecord();
            }
        }

        #region Subscriptions

        private void Subscribe()
        {
            HealthRecords.CollectionChanged += (s, e) =>
            {
                // если добавлена запись - она ещё пустая, встреча остается чистой
                if (e.Action == NotifyCollectionChangedAction.Remove && !movingToViewGroup)
                {
                    Editable.MarkDirty();
                }
            };
            Editable.Committed += Editable_Committed;
            Editable.Deleted += Editable_Deleted;
        }

        private void Editable_Deleted(object sender, EditableEventArgs e)
        {
            // удаляем записи при удалении встречи
            while (HealthRecords.Count > 0)
            {
                HealthRecords[0].Editable.DeleteCommand.Execute(null);
            }

            Editable.Deleted -= Editable_Deleted;
            Editable.Committed -= Editable_Committed;
        }

        private void Editable_Committed(object sender, EditableEventArgs e)
        {
            this.DeleteEmpty(HealthRecords);

            HealthRecords.ForAll(hr => hr.Editable.IsDirty = false); // встреча сохранена - все записи тоже
        }

        #endregion Subscriptions

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

        #region HealthRecord stuff

        public HealthRecordViewModel AddHealthRecord()
        {
            var hrVM = NewHealthRecord();

            HealthRecords.Add(hrVM);
            SelectedHealthRecord = hrVM;

            hrVM.Editable.IsEditorActive = true; // открываем запись на редактирование

            OnPropertyChanged(() => IsEmpty);
            return hrVM;
        }

        public void DeleteCheckedHealthRecords()
        {
            HealthRecords.Where(hr => hr.IsChecked).ToList().ForAll(hr => hr.Editable.DeleteCommand.Execute(null));
        }

        private HealthRecordViewModel NewHealthRecord()
        {
            var hr = appointment.AddHealthRecord();
            var hrVM = new HealthRecordViewModel(hr);
            SubscribeHR(hrVM);
            return hrVM;
        }

        private void SubscribeHR(HealthRecordViewModel hrVM)
        {
            hrVM.PropertyChanged += hr_PropertyChanged;
            hrVM.Editable.Deleted += hr_Deleted;
            hrVM.Editable.Committed += hr_Committed;
            hrVM.Editable.DirtyChanged += hr_DirtyChanged;
        }

        private void hr_Committed(object sender, EditableEventArgs e)
        {
            var hrVM = e.viewModel as HealthRecordViewModel;
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(hrVM.healthRecord);
                transaction.Commit();
            }
        }

        private void hr_Deleted(object sender, EditableEventArgs e)
        {
            var hrVM = e.viewModel as HealthRecordViewModel;

            appointment.DeleteHealthRecord(hrVM.healthRecord);

            if (SelectedHealthRecord == hrVM)
            {
                MoveHrViewSelection();
            }

            HealthRecords.Remove(hrVM);

            hrVM.PropertyChanged -= hr_PropertyChanged;
            hrVM.Editable.Deleted -= hr_Deleted;
            hrVM.Editable.Committed -= hr_Committed;
            hrVM.Editable.DirtyChanged -= hr_DirtyChanged;

            OnPropertyChanged(() => IsEmpty);
        }

        private void hr_DirtyChanged(object sender, EditableEventArgs e)
        {
            this.Send((int)EventID.HealthRecordChanged,
                    new HealthRecordChangedParams(e.viewModel as HealthRecordViewModel).Params);

            Editable.IsDirty = HealthRecords.Any(hr => hr.Editable.IsDirty);
        }

        private void hr_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var hrVM = sender as HealthRecordViewModel;
            if (e.PropertyName == "Category")
            {
                MoveToOtherViewGroup(hrVM);
            }
            else if (e.PropertyName == "IsChecked")
            {
                CheckedHealthRecords = HealthRecords.Where(hr => hr.IsChecked).Count();
            }
        }

        private void MoveToOtherViewGroup(HealthRecordViewModel hrVM)
        {
            movingToViewGroup = true;
            SelectedHealthRecord = null;
            HealthRecords.Remove(hrVM);
            HealthRecords.Add(hrVM);
            SelectedHealthRecord = hrVM;
            movingToViewGroup = false;
        }

        #endregion HealthRecord stuff

        private void MoveHrViewSelection()
        {
            // удалена выделенная запись - меняем выделение
            var i = HealthRecordsView.CurrentPosition;
            if (i == HealthRecords.Count - 1)
            {
                // удалили последную запись в списке
                HealthRecordsView.MoveCurrentToPrevious();
            }
            else
            {
                HealthRecordsView.MoveCurrentToNext();
            }
        }

        public override string ToString()
        {
            return string.Format("{0} hrs {1:d} {2}", HealthRecords.Count, DateTime, Doctor);
        }
    }
}