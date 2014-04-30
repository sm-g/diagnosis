using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class CourseViewModel : ViewModelBase
    {
        internal readonly Course course;

        private bool _appointmentsVis;
        private AppointmentViewModel _selectedAppointment;
        private DoctorViewModel _leadDoctor;
        private ICommand _addAppointment;

        public IEditable Editable { get; private set; }
        #region Model

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
                    Editable.MarkDirty();
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

        #endregion

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
                Contract.Ensures(Contract.Result<AppointmentViewModel>() != null);
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
                            AddAppointment();
                        }, () => !IsEnded));
            }
        }

        public void AddAppointment()
        {
            var appVM = NewAppointment();

            SelectedAppointment = appVM;
            Editable.CanBeDeleted = false;

            OnPropertyChanged(() => LastAppointment);

            this.Send((int)EventID.AppointmentAdded, new AppointmentAddedParams(appVM).Params);
        }

        public CourseViewModel(Course course)
        {
            Contract.Requires(course != null);

            this.course = course;

            Editable = new EditableBase(this, switchedOn: true, dirtImmunity: true);

            LeadDoctor = EntityManagers.DoctorsManager.GetByModel(course.LeadDoctor);

            var appVMs = course.Appointments.Select(app => new AppointmentViewModel(app, this)).Reverse();
            Appointments = new ObservableCollection<AppointmentViewModel>(appVMs);
            
            if (Appointments.Count > 0)
            {
                SelectedAppointment = Appointments.Last();
            }

            Editable.CanBeDirty = true;
        }

        private AppointmentViewModel NewAppointment()
        {
            var app = course.AddAppointment();
            var appVM = new AppointmentViewModel(app, this);

            Appointments.Add(appVM);
            return appVM;
        }


        public override string ToString()
        {
            return Start.ToShortDateString() + ' ' + LeadDoctor.ToString();
        }
    }
}