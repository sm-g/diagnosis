using Diagnosis.Models;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class CourseViewModel : ViewModelBase
    {
        internal readonly Course course;

        public CourseViewModel(Course course)
        {
            Contract.Requires(course != null);
            this.course = course;
            this.validatableEntity = course;

            course.PropertyChanged += course_PropertyChanged;
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
            get { return course.Start.Date; }
            set { course.Start = value; }
        }

        public bool IsEnded
        {
            get { return course.IsEnded; }
        }

        public DateTime? End
        {
            get { return course.End; }
            set { course.End = value; }
        }

        #endregion Model

        public ICommand AddAppointmentCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            course.AddAppointment(AuthorityController.CurrentDoctor);
                        });
            }
        }

        public bool NoApps
        {
            get
            {
                return course.Appointments.Count() == 0;
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

        private void course_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                course.PropertyChanged -= course_PropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}