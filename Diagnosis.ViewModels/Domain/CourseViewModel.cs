using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class CourseViewModel : ViewModelBase, IEditableNesting
    {
        internal readonly Course course;

        internal Action<AppointmentViewModel> OpenedAppointmentSetter;
        internal Func<AppointmentViewModel> OpenedAppointmentGetter;
        private AppointmentsManager appManager;

        private ObservableCollection<WithAddNew> _appointmentsWithAddNew;
        private DoctorViewModel _leadDoctor;
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
                }
            }
        }

        public ObservableCollection<AppointmentViewModel> Appointments
        {
            get
            {
                return appManager.Appointments;
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

        public WithAddNew SelectedAppointmentWithAddNew
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
                            appManager.AddAppointment();
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
                return new RelayCommand(() =>
                        {
                            appManager.AddAppointment();
                        }, () => !IsEnded);
            }
        }

        public bool IsDoctorCurrent
        {
            get
            {
                return LeadDoctor == EntityProducers.DoctorsProducer.CurrentDoctor;
            }
        }

        public CourseViewModel(Course course)
        {
            Contract.Requires(course != null);
            this.course = course;

            appManager = new AppointmentsManager(this);
            appManager.AppointmentsLoaded += (s, e) =>
            {
                Appointments.CollectionChanged += Appointments_CollectionChanged;
            };

            LeadDoctor = EntityProducers.DoctorsProducer.GetByModel(course.LeadDoctor);

            Editable = new Editable(course);
            Editable.Deleted += Editable_Deleted;
        }

        internal void AddAppointment()
        {
            appManager.AddAppointment();
        }

        /// <summary>
        /// Вызывается при смене открытого осмотра.
        /// </summary>
        internal void OnOpenedAppointmentChanged()
        {
            OnPropertyChanged("SelectedAppointmentWithAddNew");
        }

        private ObservableCollection<WithAddNew> MakeAppointmentsWithAddNew(IEnumerable<AppointmentViewModel> appVMs)
        {
            var appointmentsWithAddNew = new ObservableCollection<WithAddNew>(
                appVMs.Select(app => new WithAddNew(app)));
            if (!IsEnded)
                appointmentsWithAddNew.Add(new WithAddNew());
            return appointmentsWithAddNew;
        }

        private void Editable_Deleted(object sender, EditableEventArgs e)
        {
            Editable.Deleted -= Editable_Deleted;
            Appointments.CollectionChanged -= Appointments_CollectionChanged;
        }

        private void Appointments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // если был добавлен первый осмотр, AppointmentWithAddNew для него 
                // создаётся в MakeAppointmentsWithAddNew
                if (Appointments.Count != 1)
                {
                    foreach (AppointmentViewModel newApp in e.NewItems)
                    {
                        var newitem = new WithAddNew(newApp);
                        AppointmentsWithAddNew.Insert(AppointmentsWithAddNew.Count - 1, newitem);
                    }
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

            OnPropertyChanged("LastAppointment");
            OnPropertyChanged("IsEmpty");
        }

        public override string ToString()
        {
            return course.ToString();
        }
    }
}