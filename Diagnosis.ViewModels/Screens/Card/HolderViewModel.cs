﻿using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class HolderViewModel : ViewModelBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HolderViewModel));
        private readonly IHrsHolder holder;
        private bool _showOpen;

        public HolderViewModel(IHrsHolder holder)
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
        }

        public IHrsHolder Holder
        {
            get { return holder; }
        }

        public bool IsEmpty
        {
            get { return holder.IsEmpty(); }
        }

        public bool ShowOpen
        {
            get
            {
                return _showOpen;
            }
            set
            {
                if (_showOpen != value)
                {
                    _showOpen = value;
                    OnPropertyChanged(() => ShowOpen);
                }
            }
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
                        var app = (holder as Course).GetOrderedAppointments().LastOrDefault();
                        if (app != null)
                            this.Send(Events.OpenAppointment, app.AsParams(MessageKeys.Appointment));
                    }
                }, () => !(holder.IsEmpty() || (holder is Appointment)));
            }
        }

        private void holder_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateIsEmpty();
        }

        internal void UpdateIsEmpty()
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
    }
}