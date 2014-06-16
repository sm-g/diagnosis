using Diagnosis.Core;
using Diagnosis.Data;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class CourseViewModel : ViewModelBase, IEditableNesting
    {
        internal readonly Course course;

        internal Action<AppointmentViewModel> OpenedAppointmentSetter;
        internal Func<AppointmentViewModel> OpenedAppointmentGetter;

        private ObservableCollection<AppointmentViewModel> _appointments;
        private ObservableCollection<WithAddNew> _appointmentsWithAddNew;
        private DoctorViewModel _leadDoctor;
        private ICommand _addAppointment;
        private bool isAddingNewApp;

        #region IEditableNesting

        public Editable Editable { get; private set; }

        /// <summary>
        /// Курс пустой, если пусты все осмотры в нём.
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

        public ObservableCollection<AppointmentViewModel> Appointments
        {
            get
            {
                if (_appointments == null)
                {
                    _appointments = MakeAppointments(course);

                    AfterAppointmentsLoaded();
                }
                return _appointments;
            }
        }

        #endregion Model

        /// <summary>
        /// Осмотры вместе в кнопкой Новый осмотр.
        /// </summary>
        public ObservableCollection<WithAddNew> AppointmentsWithAddNew
        {
            get
            {
                if (_appointmentsWithAddNew == null)
                {
                    _appointmentsWithAddNew = MakeAppointmentsWithAddNew(Appointments);
                }
                return _appointmentsWithAddNew;
            }
        }

        public WithAddNew OpenedAppointmentWithAddNew
        {
            get
            {
                return AppointmentsWithAddNew.Where(wan => wan.Content == OpenedAppointmentGetter()).FirstOrDefault();
            }
            set
            {
                if (value != null)
                {
                    if (value.IsAddNew)
                    {
                        // выбрана вкладка +осмотр
                        if (!isAddingNewApp)
                        {
                            isAddingNewApp = true;
                            AddAppointment();
                            isAddingNewApp = false;
                        }
                        return;
                    }
                    OpenedAppointmentSetter(value.Content as AppointmentViewModel);
                }
                else
                {
                    OpenedAppointmentSetter(null);
                }
            }
        }

        /// <summary>
        /// Последний осмотр всегда есть в курсе. (Кроме состояния при удалении курса.)
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
        /// Добавляет осмотр, если курс не закончился.
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

            Editable = new Editable(course, switchedOn: true, dirtImmunity: true);

            LeadDoctor = EntityManagers.DoctorsManager.GetByModel(course.LeadDoctor);

            Editable.CanBeDirty = true;
            Editable.Deleted += Editable_Deleted;
        }

        /// <summary>
        /// Вызывается при смене открытого осмотра.
        /// </summary>
        internal void OnOpenedAppointmentChanged()
        {
            OnPropertyChanged("OpenedAppointmentWithAddNew");
        }

        private ObservableCollection<AppointmentViewModel> MakeAppointments(Course course)
        {
            bool single = course.Appointments.Count == 1; // единственный осмотр нельзя будет удалять

            var appVMs = course.Appointments.Select(app => new AppointmentViewModel(app, app.Doctor == this.LeadDoctor.doctor, single)).ToList();
            appVMs.ForAll(app => SubscribeApp(app));

            var appointments = new ObservableCollection<AppointmentViewModel>(appVMs);
            appointments.CollectionChanged += Appointments_CollectionChanged;

            return appointments;
        }

        private ObservableCollection<WithAddNew> MakeAppointmentsWithAddNew(IEnumerable<AppointmentViewModel> appVMs)
        {
            var appointmentsWithAddNew = new ObservableCollection<WithAddNew>(
                appVMs.Select(app => new WithAddNew(app)));
            if (!IsEnded)
                appointmentsWithAddNew.Add(new WithAddNew());
            return appointmentsWithAddNew;
        }

        private void AfterAppointmentsLoaded()
        {
            if (_appointments.Count == 0)
            {
                AddAppointment(true); // новый курс — добавляем осмотр
            }

            this.SubscribeEditableNesting(_appointments,
              onDeletedBefore: () => Contract.Requires(_appointments.All(a => a.IsEmpty)),
              innerChangedAfter: SetAppointmentsDeletable);
            SetAppointmentsDeletable();
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

            // нельзя удалять единственный непустой осмотр
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
            var app = course.AddAppointment(EntityManagers.DoctorsManager.CurrentDoctor.doctor);
            var appVM = new AppointmentViewModel(app, app.Doctor == this.LeadDoctor.doctor, firstInCourse);

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
            var app = e.entity as Appointment;
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(app);
                transaction.Commit();
            }
        }

        private void app_Deleted(object sender, EditableEventArgs e)
        {
            var app = e.entity as Appointment;
            course.DeleteAppointment(app);

            var appVM = Appointments.Where(vm => vm.appointment == app).FirstOrDefault();
            appVM.Editable.Deleted -= app_Deleted;
            appVM.Editable.Committed -= app_Committed;
            appVM.Editable.DirtyChanged -= app_DirtyChanged;

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
            return course.ToString();
        }
    }
}