using Diagnosis.App.Messaging;
using Diagnosis.Core;
using Diagnosis.Data;
using Diagnosis.Models;
using EventAggregator;
using NHibernate;
using System;
using System.Collections.Generic;
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
        #region Fileds
        internal readonly Appointment appointment;
        private readonly CourseViewModel courseVM;

        private static HrEditorViewModel _hrEditorStatic = new HrEditorViewModel();


        private DoctorViewModel _doctor;
        private bool _showHrEditor;
        private int _checkedHealthRecords;
        private HealthRecordViewModel _selectedHealthRecord;

        private ICommand _addHealthRecord;
        private ICommand _editHrCommand;
        private ICommand _deleteHealthRecords;
        private ICommand _moveHrSelection;

        private bool movingToViewGroup;
        bool movingSelected;
        #endregion

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
                    OnPropertyChanged("Doctor");
                    OnPropertyChanged("IsDoctorFromCourse");
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

        public HrEditorViewModel HealthRecordEditor { get { return _hrEditorStatic; } }

        public ICollectionView HealthRecordsView { get; private set; }

        public IList<string> HealthRecordsNames
        {
            get
            {
                return HealthRecords.Select(hr => hr.Name).ToList();
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
                        HealthRecordEditor.HealthRecord = value;
                        this.Send((int)EventID.HealthRecordSelected, new HealthRecordParams(value).Params);
                    }

                    _selectedHealthRecord = value;

                    OnPropertyChanged("SelectedHealthRecord");
                    OnPropertyChanged(() => ShowHrEditor);
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

        public ICommand EditHrCommand
        {
            get
            {
                return _editHrCommand
                   ?? (_editHrCommand = new RelayCommand(() =>
                        {
                            HealthRecordEditor.HealthRecord.Editable.ToggleEditor();
                        }, () => HealthRecordEditor.HealthRecord != null));
            }
        }
        public ICommand MoveHrSelectionCommand
        {
            get
            {
                return _moveHrSelection
                   ?? (_moveHrSelection = new RelayCommand<bool>((up) =>
                        {
                            movingSelected = true;
                            if (up)
                            {
                                HealthRecordsView.MoveCurrentToPrevious();
                                if (HealthRecordsView.IsCurrentBeforeFirst)
                                    HealthRecordsView.MoveCurrentToLast();
                            }
                            else
                            {
                                HealthRecordsView.MoveCurrentToNext();
                                if (HealthRecordsView.IsCurrentAfterLast)
                                    HealthRecordsView.MoveCurrentToFirst();
                            }
                            movingSelected = false;
                        }));
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
                    OnPropertyChanged("CheckedHealthRecords");
                }
            }
        }
        public bool ShowHrEditor
        {
            get
            {
                return HealthRecordEditor.HealthRecord != null && HealthRecordEditor.HealthRecord.Editable.IsEditorActive;
            }
        }

        public bool IsDoctorFromCourse
        {
            get
            {
                return Doctor == courseVM.LeadDoctor;
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

            this.SubscribeEditableNesting(HealthRecords,
                innerChangedMarkDirtyIf: () => !movingToViewGroup);

            SetupHealthRecordsView();
        }

        private void SetupHealthRecords(bool withFirstHr)
        {
            var hrVMs = appointment.HealthRecords.Select(hr => new HealthRecordViewModel(hr)).ToList();
            hrVMs.ForAll(hr => SubscribeHr(hr));

            if (HealthRecords != null)
            {
                HealthRecords.CollectionChanged -= HealthRecords_CollectionChanged;
            }
            HealthRecords = new ObservableCollection<HealthRecordViewModel>(hrVMs);

            if (withFirstHr && HealthRecords.Count == 0)
            {
                AddHealthRecord();
            }
            HealthRecords.CollectionChanged += HealthRecords_CollectionChanged;
        }

        void HealthRecords_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("HealthRecordsNames");
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

        #region HealthRecord stuff

        public HealthRecordViewModel AddHealthRecord()
        {
            var hrVM = NewHealthRecord();

            HealthRecords.Add(hrVM);
            SelectedHealthRecord = hrVM;

            hrVM.Editable.IsEditorActive = true; // открываем запись на редактирование

            OnPropertyChanged("IsEmpty");
            return hrVM;
        }

        public void DeleteCheckedHealthRecords()
        {
            HealthRecords.Where(hr => hr.IsChecked).ToList().ForAll(hr => hr.Editable.Delete());
        }

        private HealthRecordViewModel NewHealthRecord()
        {
            var hr = appointment.AddHealthRecord();
            var hrVM = new HealthRecordViewModel(hr);
            SubscribeHr(hrVM);
            return hrVM;
        }

        private void SubscribeHr(HealthRecordViewModel hrVM)
        {
            hrVM.PropertyChanged += hr_PropertyChanged;
            hrVM.Editable.Deleted += hr_Deleted;
            hrVM.Editable.Reverted += hr_Reverted;
            hrVM.Editable.Committed += hr_Committed;
            hrVM.Editable.DirtyChanged += hr_DirtyChanged;
            hrVM.Editable.PropertyChanged += hr_Editable_PropertyChanged;
        }

        private void UnsubscribeHr(HealthRecordViewModel hrVM)
        {
            hrVM.PropertyChanged -= hr_PropertyChanged;
            hrVM.Editable.Deleted -= hr_Deleted;
            hrVM.Editable.Reverted -= hr_Reverted;
            hrVM.Editable.Committed -= hr_Committed;
            hrVM.Editable.DirtyChanged -= hr_DirtyChanged;
            hrVM.Editable.PropertyChanged -= hr_Editable_PropertyChanged;
        }

        void hr_Editable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEditorActive")
            {
                OnPropertyChanged(() => ShowHrEditor);
            }
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

        private void hr_Reverted(object sender, EditableEventArgs e)
        {
            var hrVM = e.viewModel as HealthRecordViewModel;
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Refresh(hrVM.healthRecord);
            }
            hrVM.RefreshView();
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

            UnsubscribeHr(hrVM);

            OnPropertyChanged("IsEmpty");
        }
        private void hr_DirtyChanged(object sender, EditableEventArgs e)
        {
            this.Send((int)EventID.HealthRecordChanged,
                    new HealthRecordParams(e.viewModel as HealthRecordViewModel).Params);

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
            return appointment.ToString();
        }
    }
}