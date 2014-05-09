using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventAggregator;
using Diagnosis.App.ViewModels;
using Diagnosis.Models;

namespace Diagnosis.App
{
    class DebugOutput
    {
        public DebugOutput()
        {
            this.Subscribe((int)EventID.WordCheckedChanged, (e) =>
                   {
                       var word = e.GetValue<WordViewModel>(Messages.Word);
                       var isChecked = e.GetValue<bool>(Messages.CheckedState);
                       Console.WriteLine("word {0} {1}", word, isChecked ? "checked" : "unchecked");
                   });

            this.Subscribe((int)EventID.AppointmentAdded, (e) =>
            {
                var app = e.GetValue<AppointmentViewModel>(Messages.Appointment);
                Console.WriteLine("app {0} added", app);
            });
            this.Subscribe((int)EventID.CourseStarted, (e) =>
            {
                var c = e.GetValue<Course>(Messages.Course);
                Console.WriteLine("course {0} started", c);
            });
            this.Subscribe((int)EventID.CurrentPatientChanged, (e) =>
            {
                var p = e.GetValue<PatientViewModel>(Messages.Patient);
                Console.WriteLine("cur patient is {0}", p);
            });
            this.Subscribe((int)EventID.HealthRecordChanged, (e) =>
            {
                var hr = e.GetValue<HealthRecordViewModel>(Messages.HealthRecord);
                Console.WriteLine("hr {0} changed", hr);
            });
            this.Subscribe((int)EventID.HealthRecordSelected, (e) =>
            {
                var hr = e.GetValue<HealthRecordViewModel>(Messages.HealthRecord);
                Console.WriteLine("hr {0} selected", hr);
            });
        }
    }
}
