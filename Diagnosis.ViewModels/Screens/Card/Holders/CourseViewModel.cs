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

        public CourseViewModel(Course course)
        {
            Contract.Requires(course != null);
            this.course = course;
            this.validatableEntity = course;

            course.PropertyChanged += course_PropertyChanged;
            appManager = new AppointmentsManager(course);
        }


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
            set
            {
                course.Start = value;
            }
        }

        public bool IsEnded
        {
            get
            {
                return course.IsEnded;
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
                course.End = value;
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

        public override string ToString()
        {
            return string.Format("{0} {1}", GetType().Name, course);
        }

        internal void SelectAppointment(Appointment appointment)
        {
            SelectedAppointment = Appointments.FirstOrDefault(x => x.appointment == appointment);
        }

        private void course_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                course.PropertyChanged -= course_PropertyChanged;
                appManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}