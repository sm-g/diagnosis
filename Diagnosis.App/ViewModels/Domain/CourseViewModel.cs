using Diagnosis.App.Messaging;
using Diagnosis.Core;
using Diagnosis.Data;
using Diagnosis.Models;
using EventAggregator;
using NHibernate;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ComponentModel;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class CourseViewModel : ViewModelBase, IEditableNesting
    {
        internal readonly Course course;

        private AppointmentViewModel _selectedAppointment;
        private DoctorViewModel _leadDoctor;
        private ICommand _addAppointment;
        private bool isAddingNewApp;

        #region IEditableNesting

        public Editable Editable { get; private set; }

        /// <summary>
        /// Курс пустой, если пусты все встречи в нём.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Appointments.All(app => app.IsEmpty);
            }
        }

        #endregion IEditableNesting

        #region Model

        public DoctorViewModel LeadDoctor
        {
            get
            {
                return _leadDoctor;
            }
            set
            {
                if (_leadDoctor != value)
                {
                    _leadDoctor = value;
                    OnPropertyChanged("LeadDoctor");
                }
            }
        }

        public DateTime Start
        {
            get
            {
                return course.Start.Date;
            }
        }

        public bool IsEnded
        {
            get
            {
                return course.End.HasValue;
            }
        }

        public DateTime? End
        {
            get
            {
                return course.End;
            }
            set
            {
                if (!course.End.HasValue || course.End.Value.Date != value.Value.Date)
                {
                    course.End = value.Value.Date;
                    OnPropertyChanged("End");
                    OnPropertyChanged("IsEnded");
                    Editable.MarkDirty();
                }
            }
        }

        public ObservableCollection<AppointmentViewModel> Appointments { get; private set; }

        #endregion Model

        /// <summary>
        /// Осмотры вместе в кнопкой Новый осмотр.
        /// </summary>
        public ObservableCollection<WithAddNew> AppointmentsWithAddNew { get; private set; }

        public AppointmentViewModel SelectedAppointment
        {
            get
            {
                return _selectedAppointment;
            }
            set
            {
                if (_selectedAppointment != value)
                {
                    if (_selectedAppointment != null)
                    {
                        _selectedAppointment.Editable.Commit();
                    }

                    _selectedAppointment = value;
                    OnPropertyChanged("SelectedAppointmentWithAddNew");
                    OnPropertyChanged("SelectedAppointment");
                }
            }
        }

        public WithAddNew SelectedAppointmentWithAddNew
        {
            get
            {
                return AppointmentsWithAddNew.Where(wan => wan.Content == SelectedAppointment).FirstOrDefault();
            }
            set
            {
                if (value != null)
                {
                    if (value.IsAddNew)
                    {
                        if (!isAddingNewApp)
                        {
                            isAddingNewApp = true;
                            AddAppointment();
                            isAddingNewApp = false;
                        }
                        return;
                    }
                    SelectedAppointment = value.Content as AppointmentViewModel;
                }
                else
                {
                    SelectedAppointment = null;
                }
            }
        }

        /// <summary>
        /// Последняя встреча всегда есть в курсе. (Кроме состояния при удалении курса.)
        /// </summary>
        public AppointmentViewModel LastAppointment
        {
            get
            {
                Contract.Requires(Appointments.Count > 0);
                return Appointments.Last();
            }
        }
        /// <summary>
        /// Добавляет встречу, если курс не закончился.
        /// </summary>
        public ICommand AddAppointmentCommand
        {
            get
            {
                return _addAppointment
                    ?? (_addAppointment = new RelayCommand(() =>
                        {
                            AddAppointment();
                        }, () => !IsEnded));
            }
        }

        public bool IsDoctorCurrent
        {
            get
            {
                return LeadDoctor == EntityManagers.DoctorsManager.CurrentDoctor;
            }
        }

        public CourseViewModel(Course course)
        {
            Contract.Requires(course != null);

            this.course = course;

            Editable = new Editable(this, switchedOn: true, dirtImmunity: true);

            LeadDoctor = EntityManagers.DoctorsManager.GetByModel(course.LeadDoctor);
            SetupAppointments(course);

            Editable.CanBeDirty = true;

            this.SubscribeEditableNesting(Appointments,
                 onDeletedBefore: () => Contract.Requires(Appointments.All(a => a.IsEmpty)),
                 innerChangedAfter: SetAppointmentsDeletable);

            SetAppointmentsDeletable();
        }

        private void SetupAppointments(Course course)
        {
            bool single = course.Appointments.Count == 1; // единственную встречу нельзя будет удалять

            var appVMs = course.Appointments.Select(app => new AppointmentViewModel(app, this, single)).Reverse().ToList();
            appVMs.ForAll(app => SubscribeApp(app));

            Appointments = new ObservableCollection<AppointmentViewModel>(appVMs);
            Appointments.CollectionChanged += Appointments_CollectionChanged;

            AppointmentsWithAddNew = new ObservableCollection<WithAddNew>(
                appVMs.Select(app => new WithAddNew(app)));
            if (!IsEnded)
                AppointmentsWithAddNew.Add(new WithAddNew());

            if (Appointments.Count > 0)
            {
                SelectedAppointment = LastAppointment;
                Editable.CanBeDeleted = false; // не новый курс — встречи не пустые, удалить курс нельзя
            }
            else
            {
                AddAppointment(true); // новый курс — добавляем встречу
            }

            this.Editable.Deleted += Editable_Deleted;
        }

        private void Editable_Deleted(object sender, EditableEventArgs e)
        {
            this.Editable.Deleted -= Editable_Deleted;
            Appointments.CollectionChanged -= Appointments_CollectionChanged;
        }

        private void Appointments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (AppointmentViewModel newApp in e.NewItems)
                {
                    var newitem = new WithAddNew(newApp);
                    AppointmentsWithAddNew.Insert(AppointmentsWithAddNew.Count - 1, newitem);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (AppointmentViewModel oldApp in e.OldItems)
                {
                    var item = AppointmentsWithAddNew.FirstOrDefault(wan => wan.Content == oldApp);
                    if (item != null)
                        try
                        {
                            AppointmentsWithAddNew.Remove(item);
                        }
                        catch (IndexOutOfRangeException)
                        {
                        }
                }
                if (SelectedAppointment == null && Appointments.Count > 0)
                {
                    SelectedAppointment = LastAppointment;
                }
            }
        }

        #region Appointment stuff

        public AppointmentViewModel AddAppointment(bool firstInCourse = false)
        {
            if (!IsEnded)
            {
                var appVM = NewAppointment(firstInCourse);
                Appointments.Add(appVM);

                OnPropertyChanged("LastAppointment");
                OnPropertyChanged("IsEmpty");

                return appVM;
            }
            return null;
        }

        private void SetAppointmentsDeletable()
        {
            // проверяем, остался ли курс пустым
            Editable.CanBeDeleted = IsEmpty;

            // нельзя удалять единственную непустую встречу
            if (Appointments.Count > 1)
            {
                Appointments.ForAll(app => app.Editable.CanBeDeleted = true);
            }
            else if (Appointments.Count == 1)
            {
                Appointments.Single().Editable.CanBeDeleted = IsEmpty;
            }
        }

        private AppointmentViewModel NewAppointment(bool firstInCourse)
        {
            var app = course.AddAppointment();
            var appVM = new AppointmentViewModel(app, this, firstInCourse);

            SubscribeApp(appVM);
            return appVM;
        }

        private void SubscribeApp(AppointmentViewModel appVM)
        {
            appVM.Editable.Deleted += app_Deleted;
            appVM.Editable.Committed += app_Committed;
            appVM.Editable.DirtyChanged += app_DirtyChanged;
        }

        private void app_Committed(object sender, EditableEventArgs e)
        {
            var appVM = e.viewModel as AppointmentViewModel;
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(appVM.appointment);
                transaction.Commit();
            }
        }

        private void app_Deleted(object sender, EditableEventArgs e)
        {
            var appVM = e.viewModel as AppointmentViewModel;
            appVM.Editable.Deleted -= app_Deleted;
            appVM.Editable.Committed -= app_Committed;
            appVM.Editable.DirtyChanged -= app_DirtyChanged;

            course.DeleteAppointment(appVM.appointment);

            Appointments.Remove(appVM);

            OnPropertyChanged("IsEmpty");
        }

        private void app_DirtyChanged(object sender, EditableEventArgs e)
        {
            Editable.IsDirty = Appointments.Any(app => app.Editable.IsDirty);

            SetAppointmentsDeletable();
        }

        #endregion Appointment stuff

        public override string ToString()
        {
            return "курс " + Start.ToShortDateString() + ' ' + LeadDoctor.ToString();
        }
    }
}