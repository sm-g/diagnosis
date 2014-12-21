using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class HolderViewModel : ViewModelBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HolderViewModel));
        private readonly IHrsHolder holder;

        public HolderViewModel(IHrsHolder holder)
        {
            Contract.Requires(holder != null);
            this.holder = holder;
            if (holder is Patient)
            {
                var pat = holder as Patient;
                pat.CoursesChanged += pat_CoursesChanged;
            }
            else if (holder is Course)
            {
                var course = holder as Course;
                course.AppointmentsChanged += course_AppointmentsChanged;
            }
            else if (holder is Appointment)
            {
                var app = holder as Appointment;
            }
        }

        public IHrsHolder Holder
        {
            get { return holder; }
        }

        public bool IsEmpty
        {
            get { return holder.IsEmpty(); }
        }

        public RelayCommand OpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Events.OpenHolder, Holder.AsParams(MessageKeys.Holder));
                });
            }
        }

        public RelayCommand StartCourseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    AuthorityController.CurrentDoctor.StartCourse(Holder as Patient);
                },
                () => Holder is Patient);
            }
        }

        public RelayCommand AddAppointmentCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    (Holder as Course).AddAppointment(AuthorityController.CurrentDoctor);
                },
                () => Holder is Course && !(Holder as Course).IsEnded);
            }
        }

        public RelayCommand AddFirstAppointmentCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var course = AuthorityController.CurrentDoctor.StartCourse(Holder as Patient);
                    var app = course.AddAppointment(AuthorityController.CurrentDoctor);
                },
                () => Holder is Patient);
            }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Events.DeleteHolder, Holder.AsParams(MessageKeys.Holder));
                }, () => Holder.IsEmpty());
            }
        }

        public RelayCommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Events.EditHolder, Holder.AsParams(MessageKeys.Holder));
                });
            }
        }

        public RelayCommand OpenLastCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (holder is Patient)
                    {
                        var course = (holder as Patient).GetOrderedCourses().LastOrDefault();
                        if (course != null)
                            this.Send(Events.OpenCourse, course.AsParams(MessageKeys.Course));
                    }
                    else if (holder is Course)
                    {
                        var app = (holder as Course).Appointments.LastOrDefault();
                        if (app != null)
                            this.Send(Events.OpenAppointment, app.AsParams(MessageKeys.Appointment));
                    }
                }, () => !holder.IsEmpty() && !(holder is Appointment));
            }
        }

        private void course_AppointmentsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(() => IsEmpty);
        }

        private void pat_CoursesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(() => IsEmpty);
        }

        public override string ToString()
        {
            return string.Format("{0}. empty = {1}", Holder, IsEmpty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (holder is Patient)
                {
                    var pat = holder as Patient;
                    pat.CoursesChanged -= pat_CoursesChanged;
                }
                else if (holder is Course)
                {
                    var course = holder as Course;
                    course.AppointmentsChanged -= course_AppointmentsChanged;
                }
                else if (holder is Appointment)
                {
                    var app = holder as Appointment;
                }
            }
            base.Dispose(disposing);
        }
    }
}