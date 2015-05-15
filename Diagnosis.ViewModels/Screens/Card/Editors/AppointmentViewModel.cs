using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;

using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class AppointmentViewModel : ViewModelBase
    {
        internal readonly Appointment app;

        public AppointmentViewModel(Appointment appointment)
        {
            Contract.Requires(appointment != null);
            this.app = appointment;
            this.validatableEntity = appointment;

            app.PropertyChanged += app_PropertyChanged;

            columnToPropertyMap = new Dictionary<string, string>() {
                {"Date", "DateAndTime.Date"},
                {"Time", "DateAndTime.Date"}
            };
        }

        public Doctor Doctor
        {
            get { return app.Doctor; }
        }

        public DateTime Time
        {
            get { return app.DateAndTime; }
            set { app.DateAndTime = app.DateAndTime.Date.Add(value.TimeOfDay); }
        }

        public DateTime Date
        {
            get { return app.DateAndTime.Date; }
            set { app.DateAndTime = value.Add(app.DateAndTime.TimeOfDay); }
        }

        public DateTime DateAndTime
        {
            get { return app.DateAndTime; }
            set { app.DateAndTime = value; }
        }

        public bool IsDoctorFromCourse
        {
            get { return Doctor == app.Course.LeadDoctor; }
        }


        public override string ToString()
        {
            return string.Format("{0} {1}", GetType().Name, app);
        }
        private void app_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           // OnPropertyChanged(() => DateTimeInvalid);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                app.PropertyChanged -= app_PropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}