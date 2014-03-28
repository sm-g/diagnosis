using Diagnosis.Models;
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
        internal readonly Course course;

        private bool _appointmentsVis;
        private AppointmentViewModel _selectedAppointment;
        private DoctorViewModel _leadDoctor;
        private ICommand _addAppointment;

        public EditableBase Editable { get; private set; }

        #region CheckableBase

        public override void OnCheckedChanged()
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
                if (_appointmentsVis != value && Appointments.Count > 0)
                {
                    _appointmentsVis = value;

                    IsSelected = value;

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

        public AppointmentViewModel LastAppointment
        {
            get
            {
                return Appointments.LastOrDefault();
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
                            var appVM = new AppointmentViewModel(app);

                            Appointments.Add(appVM);
                            IsAppointmentsVisible = true;

                            OnPropertyChanged(() => LastAppointment);

                            this.Send((int)EventID.AppointmentAdded, new AppointmentAddedParams(app).Params);
                        }, () => !IsEnded));
            }
        }

        public CourseViewModel(Course course)
        {
            Contract.Requires(course != null);

            this.course = course;
            LeadDoctor = new DoctorViewModel(course.LeadDoctor);

            var appVMs = course.Appointments.
                Select(app => new AppointmentViewModel(app)).
                Reverse();
            Appointments = new ObservableCollection<AppointmentViewModel>(appVMs);

            if (Appointments.Count > 0)
            {
                SelectedAppointment = Appointments.Last();
            }

            this.PropertyChanged += CourseViewModel_PropertyChanged;

            Editable = new EditableBase();
        }

        void CourseViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                IsAppointmentsVisible = IsSelected;
            }
        }
    }
}