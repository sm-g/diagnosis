﻿using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class HeaderViewModel : ViewModelBase, IHrsHolderKeeper
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HeaderViewModel));
        private readonly IHrsHolder holder;
        private IHrsHolder _nextHolder;
        private IHrsHolder _prevHolder;

        private TimeSpan _nextSpan;
        private TimeSpan _prevSpan;

        public HeaderViewModel(IHrsHolder holder)
        {
            Contract.Requires(holder != null);
            this.holder = holder;
            HolderVm = new HolderViewModel(holder);

            if (holder is Patient)
            {
            }
            else if (holder is Course)
            {
                var course = holder as Course;
                course.Patient.CoursesChanged += Patient_CoursesChanged;
                ShowCourses();
            }
            else if (holder is Appointment)
            {
                var app = holder as Appointment;
                app.Course.AppointmentsChanged += Course_AppointmentsChanged;

                ShowApps();
            }

            ((INotifyPropertyChanged)holder).PropertyChanged += Holder_PropertyChanged;
        }

        public IHrsHolder Holder
        {
            get { return holder; }
        }

        public HolderViewModel HolderVm { get; private set; }

        public IHrsHolder NextHolder
        {
            get
            {
                return _nextHolder;
            }
            private set
            {
                if (_nextHolder != value)
                {
                    if (_nextHolder != null)
                    {
                        ((INotifyPropertyChanged)_nextHolder).PropertyChanged -= Next_PropertyChanged;
                    }
                    _nextHolder = value;
                    if (value is INotifyPropertyChanged)
                    {
                        ((INotifyPropertyChanged)_nextHolder).PropertyChanged += Next_PropertyChanged;
                    }

                    UpdateNext();
                    OnPropertyChanged(() => NextHolder);
                }
            }
        }

        public IHrsHolder PrevHolder
        {
            get
            {
                return _prevHolder;
            }
            private set
            {
                if (_prevHolder != value)
                {
                    if (_prevHolder != null)
                    {
                        ((INotifyPropertyChanged)_prevHolder).PropertyChanged -= Prev_PropertyChanged;
                    }
                    _prevHolder = value;
                    if (value is INotifyPropertyChanged)
                    {
                        ((INotifyPropertyChanged)_prevHolder).PropertyChanged += Prev_PropertyChanged;
                    }

                    UpdatePrev();
                    OnPropertyChanged(() => PrevHolder);
                }
            }
        }

        public RelayCommand OpenNextCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.OpenHolder, NextHolder.AsParams(MessageKeys.Holder));
                },
                () => NextHolder != null);
            }
        }

        public RelayCommand OpenPrevCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.OpenHolder, PrevHolder.AsParams(MessageKeys.Holder));
                },
                () => PrevHolder != null);
            }
        }

        public TimeSpan NextSpan
        {
            get
            {
                return _nextSpan;
            }
            private set
            {
                if (_nextSpan != value)
                {
                    _nextSpan = value;
                    OnPropertyChanged(() => NextSpan);
                }
            }
        }

        public TimeSpan PrevSpan
        {
            get
            {
                return _prevSpan;
            }
            private set
            {
                if (_prevSpan != value)
                {
                    _prevSpan = value;
                    OnPropertyChanged(() => PrevSpan);
                }
            }
        }

        private void Course_AppointmentsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ShowApps();
        }

        private void Patient_CoursesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ShowCourses();
        }

        private void Holder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdatePrev();
            UpdateNext();
        }

        private void Prev_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdatePrev();
        }

        private void Next_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateNext();
        }

        private void ShowApps()
        {
            var app = Holder as Appointment;
            var apps = app.Course.GetOrderedAppointments();

            var triad = apps
                    .FindTriad(a => a == app)
                    .ToList();

            PrevHolder = triad[0];
            NextHolder = triad[2];
        }

        private void ShowCourses()
        {
            var course = Holder as Course;
            var courses = course.Patient.GetOrderedCourses();

            var triad = courses
                  .FindTriad(a => a == course)
                  .ToList();

            PrevHolder = triad[0];
            NextHolder = triad[2];
        }

        private void UpdatePrev()
        {
            if (PrevHolder == null)
            {
                PrevSpan = TimeSpan.Zero;
                return;
            }
            if (Holder is Appointment)
            {
                var app = Holder as Appointment;
                var prev = PrevHolder as Appointment;
                PrevSpan = app.DateAndTime - prev.DateAndTime;
            }
            else if (Holder is Course)
            {
                var course = Holder as Course;
                var prev = PrevHolder as Course;
                PrevSpan = GetOffsetBetween(prev, course);
            }
        }

        private void UpdateNext()
        {
            if (NextHolder == null)
            {
                NextSpan = TimeSpan.Zero;
                return;
            }
            if (Holder is Appointment)
            {
                var app = Holder as Appointment;
                var next = NextHolder as Appointment;
                NextSpan = next.DateAndTime - app.DateAndTime;
            }
            else if (Holder is Course)
            {
                var course = Holder as Course;
                var next = NextHolder as Course;
                NextSpan = GetOffsetBetween(next, course);
            }
        }

        /// <summary>
        /// Время между началом позднего курса и концом или началом раннего.
        /// </summary>
        private TimeSpan GetOffsetBetween(Course c1, Course c2)
        {
            var ordered = new[] { c1, c2 }.OrderBy(c => c).ToArray();
            var f = ordered[0];
            var l = ordered[1];
            if (f.IsEnded && l.Start >= f.End.Value)
            {
                // первый закончился не позже начала второго
                return l.Start - f.End.Value;
            }
            return l.Start - f.Start;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PrevHolder = null;
                NextHolder = null;

                if (holder is Course)
                {
                    var course = holder as Course;
                    course.Patient.CoursesChanged -= Patient_CoursesChanged;
                }
                else if (holder is Appointment)
                {
                    var app = holder as Appointment;
                    app.Course.AppointmentsChanged -= Course_AppointmentsChanged;
                }

                ((INotifyPropertyChanged)holder).PropertyChanged -= Holder_PropertyChanged;

                HolderVm.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}