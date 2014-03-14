﻿using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels
{
    public class CourseViewModel : CheckableBase
    {
        private Course course;
        private bool _appointmentsVis;
        private DoctorViewModel _leadDoctor;

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
            set
            {
                if (course.Start.Date != value.Date)
                {
                    course.Start = value.Date;
                    OnPropertyChanged(() => Start);
                }
            }
        }

        public DateTime End
        {
            get
            {
                return course.End.Date;
            }
            set
            {
                if (course.End.Date != value.Date)
                {
                    course.End = value.Date;
                    OnPropertyChanged(() => End);
                }
            }
        }

        public ObservableCollection<AppointmentViewModel> Appointments { get; private set; }

        public CourseViewModel(Course course)
        {
            Contract.Requires(course != null);

            this.course = course;
            LeadDoctor = new DoctorViewModel(course.LeadDoctor);
            Appointments = new ObservableCollection<AppointmentViewModel>();
        }
    }
}