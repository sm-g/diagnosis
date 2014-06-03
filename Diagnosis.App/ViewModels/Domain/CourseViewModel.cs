﻿using Diagnosis.App.Messaging;
using Diagnosis.Core;
using Diagnosis.Data;
using Diagnosis.Models;
using EventAggregator;
using NHibernate;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class CourseViewModel : ViewModelBase, IEditableNesting
    {
        internal readonly Course course;

        private AppointmentViewModel _selectedAppointment;
        private DoctorViewModel _leadDoctor;
        private ICommand _addAppointment;

        #region IEditableNesting

        public Editable Editable { get; private set; }

        /// <summary>
        /// Курс пустой, если пусты все встречи в нём.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Appointments.All(app => app.IsEmpty);
            }
        }

        #endregion IEditableNesting

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
                    Editable.MarkDirty();
                }
            }
        }

        public ObservableCollection<AppointmentViewModel> Appointments { get; private set; }

        #endregion Model

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
                    if (_selectedAppointment != null)
                    {
                        _selectedAppointment.Editable.Commit();
                    }

                    _selectedAppointment = value;
                    OnPropertyChanged(() => SelectedAppointment);
                }
            }
        }

        /// <summary>
        /// Последняя встреча всегда есть в курсе. (Кроме состояния при удалении курса.)
        /// </summary>
        public AppointmentViewModel LastAppointment
        {
            get
            {
                Contract.Requires(Appointments.Count > 0);
                return Appointments.Last();
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

        public CourseViewModel(Course course)
        {
            Contract.Requires(course != null);

            this.course = course;

            Editable = new Editable(this, switchedOn: true, dirtImmunity: true);

            LeadDoctor = EntityManagers.DoctorsManager.GetByModel(course.LeadDoctor);
            SetupAppointments(course);

            Editable.CanBeDirty = true;

            Subscribe();
        }

        private void SetupAppointments(Course course)
        {
            bool single = course.Appointments.Count == 1; // единственную встречу нельзя будет удалять

            var appVMs = course.Appointments.Select(app => new AppointmentViewModel(app, this, single)).Reverse().ToList();
            appVMs.ForAll(app => SubscribeApp(app));

            Appointments = new ObservableCollection<AppointmentViewModel>(appVMs);

            if (Appointments.Count > 0)
            {
                SelectedAppointment = Appointments.Last();
                Editable.CanBeDeleted = false; // не новый курс — встречи не пустые, удалить курс нельзя
            }
            else
            {
                AddAppointment(true); // новый курс — добавляем встречу
            }
        }

        #region Subscriptions

        private void Subscribe()
        {
            Editable.Committed += Editable_Committed;
            Editable.Deleted += Editable_Deleted;
            Appointments.CollectionChanged += (s, e) =>
            {
                // добавленная встреча сначала пустая
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    Editable.MarkDirty();
                }
                SetAppointmentsDeletable();
            };
        }

        private void Editable_Deleted(object sender, EditableEventArgs e)
        {
            Contract.Requires(Appointments.All(a => a.IsEmpty));
            // удаляем встречи при удалении курса (должны быть пустые)
            while (Appointments.Count > 0)
            {
                Appointments[0].Editable.Delete();
            }

            Editable.Committed -= Editable_Committed;
            Editable.Deleted -= Editable_Deleted;
        }

        private void Editable_Committed(object sender, EditableEventArgs e)
        {
            this.DeleteEmpty(Appointments);
            Appointments.ForAll(app => app.Editable.Commit());
        }

        #endregion Subscriptions

        #region Appointment stuff

        public AppointmentViewModel AddAppointment(bool firstInCourse = false)
        {
            var appVM = NewAppointment(firstInCourse);
            Appointments.Add(appVM);
            SelectedAppointment = appVM;

            OnPropertyChanged(() => LastAppointment);
            OnPropertyChanged(() => IsEmpty);

            this.Send((int)EventID.AppointmentAdded, new AppointmentAddedParams(appVM).Params);

            return appVM;
        }

        private void SetAppointmentsDeletable()
        {
            // нельзя удалять единственную встречу
            if (Appointments.Count > 1)
            {
                Appointments.ForAll(app => app.Editable.CanBeDeleted = true);
            }
            else if (Appointments.Count == 1)
            {
                Appointments.Single().Editable.CanBeDeleted = false;
            }
        }

        private AppointmentViewModel NewAppointment(bool firstInCourse)
        {
            var app = course.AddAppointment();
            var appVM = new AppointmentViewModel(app, this, firstInCourse);

            SubscribeApp(appVM);
            return appVM;
        }

        private void SubscribeApp(AppointmentViewModel appVM)
        {
            appVM.Editable.Deleted += app_Deleted;
            appVM.Editable.Committed += app_Committed;
            appVM.Editable.DirtyChanged += app_DirtyChanged;
        }

        private void app_Committed(object sender, EditableEventArgs e)
        {
            var appVM = e.viewModel as AppointmentViewModel;
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(appVM.appointment);
                transaction.Commit();
            }

            Editable.IsDirty = Appointments.Any(app => app.Editable.IsDirty);
        }

        private void app_Deleted(object sender, EditableEventArgs e)
        {
            var appVM = e.viewModel as AppointmentViewModel;
            appVM.Editable.Deleted -= app_Deleted;
            appVM.Editable.Committed -= app_Committed;
            appVM.Editable.DirtyChanged -= app_DirtyChanged;

            course.DeleteAppointment(appVM.appointment);

            Appointments.Remove(appVM);

            OnPropertyChanged(() => IsEmpty);
        }

        private void app_DirtyChanged(object sender, EditableEventArgs e)
        {
            Editable.IsDirty = Appointments.Any(app => app.Editable.IsDirty);

            // проверяем, остался ли курс пустым
            Editable.CanBeDeleted = IsEmpty;
        }

        #endregion Appointment stuff

        public override string ToString()
        {
            return "курс " + Start.ToShortDateString() + ' ' + LeadDoctor.ToString();
        }
    }
}