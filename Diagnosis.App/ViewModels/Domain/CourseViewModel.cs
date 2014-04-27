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

                    IsSelected = value;

                    OnPropertyChanged(() => IsAppointmentsVisible);
                }
            }
        }

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
                            var appVM = AddAppointment();

                            SelectedAppointment = appVM;
                            IsAppointmentsVisible = true;
                            Editable.CanBeDeleted = false;

                            OnPropertyChanged(() => LastAppointment);

                            this.Send((int)EventID.AppointmentAdded, new AppointmentAddedParams(appVM).Params);
                        }, () => !IsEnded));
            }
        }

        public CourseViewModel(Course course)
        {
            Contract.Requires(course != null);

            this.course = course;

            Editable = new EditableBase(this, switchedOn: true, dirtImmunity: true);

            LeadDoctor = EntityManagers.DoctorsManager.GetByModel(course.LeadDoctor);

            var appVMs = course.Appointments.Select(app => new AppointmentViewModel(app, this)).Reverse();
            Appointments = new ObservableCollection<AppointmentViewModel>(appVMs);

            this.PropertyChanged += CourseViewModel_PropertyChanged;

            if (Appointments.Count > 0)
            {
                SelectedAppointment = Appointments.Last();
            }

            Editable.CanBeDirty = true;
        }

        #region CheckableBase

        public override void OnCheckedChanged()
        {
            throw new NotImplementedException();
        }

        #endregion CheckableBase

        private AppointmentViewModel AddAppointment()
        {
            var app = course.AddAppointment();
            var appVM = new AppointmentViewModel(app, this);

            Appointments.Add(appVM);
            return appVM;
        }

        private void CourseViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                IsAppointmentsVisible = IsSelected;
            }
        }

        public override string ToString()
        {
            return Start.ToShortDateString() + ' ' + LeadDoctor.ToString();
        }
    }
}