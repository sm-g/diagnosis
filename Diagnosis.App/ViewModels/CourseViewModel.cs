﻿using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class CourseViewModel : CheckableBase
    {
        private Course course;

        private bool _appointmentsVis;
        private AppointmentViewModel _selectedAppointment;
        private DoctorViewModel _leadDoctor;
        private ICommand _addAppointment;

        #region CheckableBase

        public override string Name
        {
            get
            {
                return "Курс";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected override void OnCheckedChanged()
        {
            throw new NotImplementedException();
        }

        #endregion CheckableBase

        public bool IsAppointmentsVisible
        {
            get
            {
                return _appointmentsVis;
            }
            set
            {
                if (_appointmentsVis != value)
                {
                    _appointmentsVis = value;
                    OnPropertyChanged(() => IsAppointmentsVisible);
                }
            }
        }

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
                    OnPropertyChanged(() => LeadDoctor);
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

        public DateTime End
        {
            get
            {
                return course.End.HasValue ? course.End.Value.Date : new DateTime();
            }
            set
            {
                if (!course.End.HasValue || course.End.Value.Date != value.Date)
                {
                    course.End = value.Date;
                    OnPropertyChanged(() => End);
                    OnPropertyChanged(() => IsEnded);
                }
            }
        }

        public ObservableCollection<AppointmentViewModel> Appointments { get; private set; }

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
                    _selectedAppointment = value;
                    OnPropertyChanged(() => SelectedAppointment);
                }
            }
        }

        public ICommand AddAppointmentCommand
        {
            get
            {
                return _addAppointment
                    ?? (_addAppointment = new RelayCommand(() =>
                        {
                            var app = course.AddAppointment();
                            Appointments.Add(new AppointmentViewModel(app));

                            this.Send((int)EventID.AppointmentAdded, new AppointmentAddedParams(app).Params);
                        }));
            }
        }

        public CourseViewModel(Course course)
        {
            Contract.Requires(course != null);

            this.course = course;
            LeadDoctor = new DoctorViewModel(course.LeadDoctor);
            Appointments = new ObservableCollection<AppointmentViewModel>(course.Appointments.Select(app => new AppointmentViewModel(app)));

            if (Appointments.Count > 0)
            {
                SelectedAppointment = Appointments.Last();
            }
        }
    }
}