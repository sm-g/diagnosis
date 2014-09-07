﻿using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class ShortCourseViewModel : ViewModelBase
    {
        internal readonly Course course;

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
        public bool IsDoctorCurrent
        {
            get
            {
                return LeadDoctor == AuthorityController.CurrentDoctor;
            }
        }

        public ShortCourseViewModel(Course course)
        {
            Contract.Requires(course != null);
            this.course = course;


        }

        public override string ToString()
        {
            return course.ToString();
        }
    }
}