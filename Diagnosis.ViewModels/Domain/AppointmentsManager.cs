using Diagnosis.Core;
using Diagnosis.Data;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Diagnostics;

namespace Diagnosis.ViewModels
{
    public class AppointmentsManager
    {
        private readonly CourseViewModel courseVM;
        private ObservableCollection<AppointmentViewModel> _appointments;

        public event EventHandler AppointmentsLoaded;

        public ObservableCollection<AppointmentViewModel> Appointments
        {
            get
            {
                if (_appointments == null)
                {
                    _appointments = MakeAppointments();

                    courseVM.SubscribeEditableNesting(_appointments,
                        onDeletedBefore: () => Contract.Requires(_appointments.All(a => a.IsEmpty)),
                        innerChangedAfter: SetAppointmentsDeletable);
                    SetAppointmentsDeletable();
                    OnAppointmentsLoaded();
                }
                return _appointments;
            }
        }

        public AppointmentsManager(CourseViewModel courseVM)
        {
            this.courseVM = courseVM;
        }

        public AppointmentViewModel AddAppointment()
        {
            if (!courseVM.IsEnded)
            {
                var app = courseVM.course.AddAppointment(EntityProducers.DoctorsProducer.CurrentDoctor.doctor);
                var appVM = MakeAppointmentVM(app);
                Appointments.Add(appVM);

                return appVM;
            }
            return null;
        }

        protected virtual void OnAppointmentsLoaded()
        {
            var h = AppointmentsLoaded;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        private ObservableCollection<AppointmentViewModel> MakeAppointments()
        {
            IList<AppointmentViewModel> appVMs;
            using (var tester = new PerformanceTester((ts) => Debug.Print("making apps for {0}: {1}", courseVM, ts)))
            {
                appVMs = courseVM.course.Appointments.Select(app => new AppointmentViewModel(app, app.Doctor == courseVM.LeadDoctor.doctor)).ToList();
            }
            appVMs.ForAll(app => SubscribeApp(app));

            var appointments = new ObservableCollection<AppointmentViewModel>(appVMs);

            return appointments;
        }

        private void SetAppointmentsDeletable()
        {
            // проверяем, остался ли курс пустым
            courseVM.Editable.CanBeDeleted = courseVM.IsEmpty;

            // нельзя удалять единственный непустой осмотр
            if (Appointments.Count > 1)
            {
                Appointments.ForAll(app => app.Editable.CanBeDeleted = true);
            }
            else if (Appointments.Count == 1)
            {
                Appointments[0].Editable.CanBeDeleted = Appointments[0].IsEmpty;
            }
        }

        private AppointmentViewModel MakeAppointmentVM(Appointment app)
        {
            var appVM = new AppointmentViewModel(app, app.Doctor == courseVM.LeadDoctor.doctor);

            SubscribeApp(appVM);
            return appVM;
        }

        private void SubscribeApp(AppointmentViewModel appVM)
        {
            appVM.Editable.Deleted += app_Deleted;
            appVM.Editable.Committed += app_Committed;
            appVM.Editable.DirtyChanged += app_DirtyChanged;
        }

        private void UnsubscribeApp(AppointmentViewModel appVM)
        {
            appVM.Editable.Deleted -= app_Deleted;
            appVM.Editable.Committed -= app_Committed;
            appVM.Editable.DirtyChanged -= app_DirtyChanged;
        }

        private void app_Committed(object sender, EditableEventArgs e)
        {
            var app = e.entity as Appointment;
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(app);
                transaction.Commit();
            }
        }

        private void app_Deleted(object sender, EditableEventArgs e)
        {
            var app = e.entity as Appointment;
            courseVM.course.DeleteAppointment(app);

            var appVM = Appointments.Where(vm => vm.appointment == app).FirstOrDefault();
            Appointments.Remove(appVM);
            UnsubscribeApp(appVM);
        }

        private void app_DirtyChanged(object sender, EditableEventArgs e)
        {
            courseVM.Editable.IsDirty = Appointments.Any(app => app.Editable.IsDirty);

            SetAppointmentsDeletable();
        }
    }
}