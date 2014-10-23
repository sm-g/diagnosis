using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class CourseViewModel : ViewModelBase
    {
        internal readonly Course course;
        private ShortAppointmentViewModel _selApp;
        private AppointmentsManager appManager;

        #region Model

        public Doctor LeadDoctor
        {
            get
            {
                return course.LeadDoctor;
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

        #endregion Model

        public ObservableCollection<ShortAppointmentViewModel> Appointments
        {
            get
            {
                return appManager.Appointments;
            }
        }

        public ShortAppointmentViewModel SelectedAppointment
        {
            get
            {
                return _selApp;
            }
            set
            {
                if (_selApp != value)
                {
                    _selApp = value;
                    OnPropertyChanged(() => SelectedAppointment);
                }
            }
        }

        public ICommand AddAppointmentCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            course.AddAppointment(AuthorityController.CurrentDoctor);
                        }, () => !IsEnded);
            }
        }

        public bool NoApps
        {
            get
            {
                return Appointments.Count == 0;
            }
        }
        public bool IsDoctorCurrent
        {
            get
            {
                return LeadDoctor == AuthorityController.CurrentDoctor;
            }
        }

        public CourseViewModel(Course course)
        {
            Contract.Requires(course != null);
            this.course = course;

            appManager = new AppointmentsManager(course);
        }

        /// <summary>
        /// Вызывается при смене открытого осмотра.
        /// </summary>
        internal void OnOpenedAppointmentChanged()
        {
            OnPropertyChanged("SelectedAppointmentWithAddNew");
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", GetType().Name, course);
        }

        internal void SelectAppointment(Appointment appointment)
        {
            SelectedAppointment = Appointments.FirstOrDefault(x => x.appointment == appointment);
        }
    }
}