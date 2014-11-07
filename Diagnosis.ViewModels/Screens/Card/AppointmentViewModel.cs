using Diagnosis.Models;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class AppointmentViewModel : ViewModelBase
    {
        internal readonly Appointment appointment;

        public AppointmentViewModel(Appointment appointment)
        {
            Contract.Requires(appointment != null);
            this.appointment = appointment;
        }

        public Doctor Doctor
        {
            get { return appointment.Doctor; }
        }

        public DateTime DateAndTime
        {
            get { return appointment.DateAndTime; }
            set { appointment.DateAndTime = value; }
        }

        public bool IsDoctorFromCourse
        {
            get { return Doctor == appointment.Course.LeadDoctor; }
        }

        public override string this[string columnName]
        {
            get
            {
                var results = appointment.SelfValidate();
                if (results == null)
                    return string.Empty;
                var message = results.Errors
                    .Where(x => x.PropertyName == columnName)
                    .Select(x => x.ErrorMessage)
                    .FirstOrDefault();
                return message != null ? message : string.Empty;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", GetType().Name, appointment);
        }
    }
}