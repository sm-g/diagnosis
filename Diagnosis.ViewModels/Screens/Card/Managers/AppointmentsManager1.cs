using Diagnosis.Core;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class AppointmentsManager1 : DisposableBase
    {
        private readonly Course course;
        private ObservableCollection<ShortAppointmentViewModel> _appointments;

        public event EventHandler AppointmentsLoaded;

        public ObservableCollection<ShortAppointmentViewModel> Appointments
        {
            get
            {
                if (_appointments == null)
                {
                    var appVMs = course.Appointments.Select(app => new ShortAppointmentViewModel(app, app.Doctor == course.LeadDoctor));

                    _appointments = new ObservableCollection<ShortAppointmentViewModel>(appVMs);

                    OnAppointmentsLoaded();
                }
                return _appointments;
            }
        }

        public AppointmentsManager1(Course course)
        {
            this.course = course;

            Appointments.Count();

            course.AppointmentsChanged += course_AppointmentsChanged;
        }

        private void course_AppointmentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (Appointment item in e.NewItems)
                {
                    var appVM = new ShortAppointmentViewModel(item, item.Doctor == course.LeadDoctor);

                    // TODO start course from card bug
                    Appointments.Insert(Appointments.Count - 1, appVM);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Appointment item in e.OldItems)
                {
                    var appVM = Appointments.Where(w => w.appointment == item).FirstOrDefault();
                    Appointments.Remove(appVM);
                }
            }
        }

        protected virtual void OnAppointmentsLoaded()
        {
            var h = AppointmentsLoaded;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    course.AppointmentsChanged -= course_AppointmentsChanged;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}