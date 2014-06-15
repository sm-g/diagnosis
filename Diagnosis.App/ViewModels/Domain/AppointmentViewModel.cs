using Diagnosis.Core;
using Diagnosis.Data;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.ObjectModel;
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

        private static HrEditorViewModel _hrEditorStatic = new HrEditorViewModel();

        private DoctorViewModel _doctor;
        private int _checkedHealthRecords;

        private ICommand _addHealthRecord;
        private ICommand _editHrCommand;
        private ICommand _deleteHealthRecords;
        private ICommand _moveHrSelection;

        private bool movingSelected;
        internal Func<HealthRecordViewModel> OpenedHrGetter;
        internal Action<HealthRecordViewModel> OpenedHrSetter;

        #endregion Fileds

        #region IEditableNesting

        public Editable Editable { get; private set; }

        /// <summary>
        /// Осмотр пуст, если пусты все записи или их нет.
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

        public HealthRecordViewModel SelectedHealthRecord
        {
            get
            {
                return OpenedHrGetter != null ? OpenedHrGetter() : null;
            }
            set
            {
                OpenedHrSetter(value);
            }
        }

        #region Commands

        public ICommand AddHealthRecordCommand
        {
            get
            {
                return _addHealthRecord
                    ?? (_addHealthRecord = new RelayCommand(() =>
                    {
                        AddHealthRecord();
                    },
                    // нельзя добавлять новую запись, пока редактируемая пуста
                    () => !(HealthRecordEditor.IsActive && HealthRecordEditor.HealthRecord.IsEmpty)));
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
                                if (HealthRecordsView.CurrentPosition != 0)
                                    HealthRecordsView.MoveCurrentToPrevious();
                                else
                                    HealthRecordsView.MoveCurrentToLast();
                            }
                            else
                            {
                                if (HealthRecordsView.CurrentPosition != HealthRecords.Count - 1)
                                    HealthRecordsView.MoveCurrentToNext();
                                else
                                    HealthRecordsView.MoveCurrentToFirst();
                            }
                            movingSelected = false;
                        }));
            }
        }

        #endregion Commands

        public int CheckedHealthRecords
        {
            get
            {
                return HealthRecords.Where(hr => hr.IsChecked).Count();
            }
        }

        public bool IsDoctorFromCourse
        {
            get;
            private set;
        }

        public AppointmentViewModel(Appointment appointment, bool doctorFromCourse, bool firstInCourse = false)
        {
            Contract.Requires(appointment != null);

            this.appointment = appointment;
            IsDoctorFromCourse = doctorFromCourse;

            appointment.PropertyChanged += appointment_PropertyChanged;
            Editable = new Editable(appointment, dirtImmunity: true, switchedOn: true);
            if (firstInCourse)
            {
                Editable.CanBeDeleted = false;
            }

            Doctor = EntityManagers.DoctorsManager.GetByModel(appointment.Doctor);
            SetupHealthRecords();

            Editable.CanBeDirty = true;

            this.SubscribeEditableNesting(HealthRecords);
        }

        internal void OnOpenedHealthRecordChanged()
        {
            if (SelectedHealthRecord != null)
            {
                SelectedHealthRecord.IsSelected = true;
            }
            HealthRecordEditor.HealthRecord = SelectedHealthRecord;
            OnPropertyChanged("SelectedHealthRecord");
        }

        private void appointment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // у осмотра может меняться только набор записей
            if (e.PropertyName == "HealthRecords")
            {
                // добавленные записи
                var notInVmCollection = appointment.HealthRecords.Where(
                    hr => !HealthRecords.Any(hrVM => hrVM.healthRecord == hr)).ToList();

                foreach (var hr in notInVmCollection)
                {
                    var hrVM = new HealthRecordViewModel(hr);
                    SubscribeHr(hrVM);
                    HealthRecords.Add(hrVM);
                }

                // удалённые записи
                var deletedVms = HealthRecords.Where(
                    hrVM => !appointment.HealthRecords.Any(hr => hrVM.healthRecord == hr)).ToList();

                foreach (var hrVM in deletedVms)
                {
                    if (SelectedHealthRecord == hrVM)
                    {
                        MoveHrViewSelection();
                    }
                    HealthRecords.Remove(hrVM);
                    UnsubscribeHr(hrVM);
                }
                OnPropertyChanged("IsEmpty");
            }
        }
        private void SetupHealthRecords()
        {
            var hrVMs = appointment.HealthRecords.Select(hr => new HealthRecordViewModel(hr)).ToList();
            hrVMs.ForAll(hr => SubscribeHr(hr));

            HealthRecords = new ObservableCollection<HealthRecordViewModel>(hrVMs);
            OnPropertyChanged("HealthRecords");

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
            OnPropertyChanged("HealthRecordsView");
        }

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

        #region HealthRecord stuff

        public void AddHealthRecord()
        {
            var last = SelectedHealthRecord ?? HealthRecords.LastOrDefault();
            var hr = appointment.AddHealthRecord();
            if (last != null)
            {
                // копируем категории из последней записи
                hr.Category = last.healthRecord.Category;
            }
        }

        public void DeleteCheckedHealthRecords()
        {
            HealthRecords.Where(hr => hr.IsChecked).ToList().ForAll(hr =>
            {
                hr.Editable.Delete();
                hr.IsChecked = false;
            });
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
        }

        private void hr_Editable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void hr_Committed(object sender, EditableEventArgs e)
        {
            var hr = e.entity as HealthRecord;
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(hr);
                transaction.Commit();
            }
        }

        private void hr_Reverted(object sender, EditableEventArgs e)
        {
            var hr = e.entity as HealthRecord;
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Refresh(hr);
            }
            var hrVM = HealthRecords.Where(vm => vm.healthRecord == hr).FirstOrDefault();
            hrVM.RefreshView();
        }

        private void hr_Deleted(object sender, EditableEventArgs e)
        {
            var hr = e.entity as HealthRecord;
            appointment.DeleteHealthRecord(hr);
        }

        private void hr_DirtyChanged(object sender, EditableEventArgs e)
        {
            Editable.IsDirty = HealthRecords.Any(vm => vm.Editable.IsDirty);
        }

        private void hr_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var hrVM = sender as HealthRecordViewModel;
            if (e.PropertyName == "Category")
            {
                HealthRecordsView.Refresh();
            }
            else if (e.PropertyName == "IsChecked")
            {
                OnPropertyChanged("CheckedHealthRecords");
            }
        }

        #endregion HealthRecord stuff
        public override string ToString()
        {
            return appointment.ToString();
        }
    }
}