using Diagnosis.Core;
using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class AppointmentViewModel : ViewModelBase
    {
        internal readonly Appointment appointment;

        public Doctor Doctor
        {
            get { return appointment.Doctor; }
        }

        public DateTime DateTime
        {
            get { return appointment.DateAndTime; }
        }

        public bool IsDoctorFromCourse
        {
            get { return Doctor == appointment.Course.LeadDoctor; }
        }

        public AppointmentViewModel(Appointment appointment)
        {
            Contract.Requires(appointment != null);
            this.appointment = appointment;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", GetType().Name, appointment);
        }
    }
}