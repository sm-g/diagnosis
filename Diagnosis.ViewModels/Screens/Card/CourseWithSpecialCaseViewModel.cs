﻿using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class CourseWithSpecialCaseViewModel : ViewModelBase
    {
        internal readonly Course course;
        private bool addingApp;
        private SpecialCaseItem _selApp;
        private AppointmentsSpecialCaseManager appManager;

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
                if (!course.IsEnded || course.End.Value.Date != value.Value.Date)
                {
                    course.End = value.Value.Date;
                    OnPropertyChanged("End");
                    OnPropertyChanged("IsEnded");
                }
            }
        }

        #endregion Model

        public ObservableCollection<SpecialCaseItem> Appointments
        {
            get
            {
                return appManager.Appointments;
            }
        }

        public SpecialCaseItem SelectedAppointment
        {
            get
            {
                return _selApp;
            }
            set
            {
                if (_selApp != value)
                {
                    if (value.IsAddNew)
                    {
                        AddAppointmentCommand.Execute(null);
                    }
                    else
                    {
                        _selApp = value;
                        OnPropertyChanged(() => SelectedAppointment);
                    }
                }
            }
        }

        /// <summary>
        /// Добавляет осмотр, если курс не закончился.
        /// </summary>
        public ICommand AddAppointmentCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            if (!addingApp)
                            {
                                addingApp = true;
                                var app = course.AddAppointment(AuthorityController.CurrentDoctor);
                                addingApp = false;
                            }
                        }, () => !IsEnded);
            }
        }

        public bool IsDoctorCurrent
        {
            get
            {
                return LeadDoctor == AuthorityController.CurrentDoctor;
            }
        }

        public CourseWithSpecialCaseViewModel(Course course)
        {
            Contract.Requires(course != null);
            this.course = course;

            appManager = new AppointmentsSpecialCaseManager(course);
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
            SelectedAppointment = Appointments.FirstOrDefault(x => x.Content != null && x.To<ShortAppointmentViewModel>().appointment == appointment);
        }
    }
}