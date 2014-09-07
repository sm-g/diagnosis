using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class CourseViewModel : ViewModelBase
    {
        internal readonly Course course;
        private AppointmentsManager appManager;

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

        public ObservableCollection<SpecialCaseItem> Appointments
        {
            get
            {
                return appManager.Appointments;
            }
        }

        #endregion Model
        private SpecialCaseItem _selApp;
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
                        var app = course.AddAppointment(AuthorityController.CurrentDoctor);
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
                            var app = course.AddAppointment(AuthorityController.CurrentDoctor);
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

        public CourseViewModel(Course course)
        {
            Contract.Requires(course != null);
            this.course = course;

            appManager = new AppointmentsManager(course);
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
            return course.ToString();
        }

        internal void SelectAppointment(Appointment appointment)
        {
            SelectedAppointment = Appointments.First(x => x.To<ShortAppointmentViewModel>().appointment == appointment);
        }
    }
}