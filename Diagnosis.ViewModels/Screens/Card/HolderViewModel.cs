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
        private VisibleRelayCommand startCourse;
        private VisibleRelayCommand open;
        private VisibleRelayCommand addAppointment;

        public HolderViewModel(IHrsHolder holder, bool showOpen = false)
        {
            Contract.Requires(holder != null);
            this.holder = holder;
            if (holder is Patient)
            {
                var pat = holder as Patient;
                pat.CoursesChanged += holder_CollectionChanged;
                pat.HealthRecordsChanged += holder_CollectionChanged;
            }
            else if (holder is Course)
            {
                var course = holder as Course;
                course.AppointmentsChanged += holder_CollectionChanged;
                course.HealthRecordsChanged += holder_CollectionChanged;
            }
            else if (holder is Appointment)
            {
                var app = holder as Appointment;
                app.HealthRecordsChanged += holder_CollectionChanged;
            }

            StartCourseCommand.IsVisible = holder is Patient;
            AddAppointmentCommand.IsVisible = holder is Course;
            OpenCommand.IsVisible = showOpen;
        }

        public IHrsHolder Holder
        {
            get { return holder; }
        }

        public bool IsEmpty
        {
            get { return holder.IsEmpty(); }
        }

        public VisibleRelayCommand OpenCommand
        {
            get
            {
                return open ?? (open = new VisibleRelayCommand(() =>
                {
                    this.Send(Event.OpenHolder, Holder.AsParams(MessageKeys.Holder));
                }));
            }
        }

        public VisibleRelayCommand StartCourseCommand
        {
            get
            {
                return startCourse ?? (startCourse = new VisibleRelayCommand(() =>
                {
                    (Holder as Patient).AddCourse(AuthorityController.CurrentDoctor);
                },
                () => Holder is Patient));
            }
        }

        public VisibleRelayCommand AddAppointmentCommand
        {
            get
            {
                return addAppointment ?? (addAppointment = new VisibleRelayCommand(() =>
                {
                    (Holder as Course).AddAppointment(AuthorityController.CurrentDoctor);
                },
                () => Holder is Course));
            }
        }

        public VisibleRelayCommand InsertHolderCommand
        {
            get
            {
                return Holder is Patient ? StartCourseCommand : AddAppointmentCommand;
            }
        }

        public RelayCommand AddFirstAppointmentCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var course = (Holder as Patient).AddCourse(AuthorityController.CurrentDoctor);
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
                    this.Send(Event.DeleteHolder, Holder.AsParams(MessageKeys.Holder));
                }, () => Holder.IsEmpty());
            }
        }

        public RelayCommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.EditHolder, Holder.AsParams(MessageKeys.Holder));
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
                            this.Send(Event.OpenCourse, course.AsParams(MessageKeys.Course));
                    }
                    else if (holder is Course)
                    {
                        var app = (holder as Course).GetOrderedAppointments().LastOrDefault();
                        if (app != null)
                            this.Send(Event.OpenAppointment, app.AsParams(MessageKeys.Appointment));
                    }
                }, () => !(holder.IsEmpty() || (holder is Appointment)));
            }
        }

        public RelayCommand AddHealthRecordCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var stratEdit = true;
                    this.Send(Event.AddHr, new object[] { holder, stratEdit }.AsParams(MessageKeys.Holder, MessageKeys.Boolean));
                });
            }
        }

        public override string ToString()
        {
            return string.Format("{0}. empty = {1}", Holder, IsEmpty);
        }

        internal void UpdateIsEmpty()
        {
            OnPropertyChanged(() => IsEmpty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (holder is Patient)
                {
                    var pat = holder as Patient;
                    pat.CoursesChanged -= holder_CollectionChanged;
                    pat.HealthRecordsChanged -= holder_CollectionChanged;
                }
                else if (holder is Course)
                {
                    var course = holder as Course;
                    course.AppointmentsChanged -= holder_CollectionChanged;
                    course.HealthRecordsChanged -= holder_CollectionChanged;
                }
                else if (holder is Appointment)
                {
                    var app = holder as Appointment;
                    app.HealthRecordsChanged -= holder_CollectionChanged;
                }
            }
            base.Dispose(disposing);
        }

        private void holder_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateIsEmpty();
        }
    }
}