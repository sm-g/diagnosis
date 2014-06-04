using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventAggregator;
using Diagnosis.App.ViewModels;
using Diagnosis.Models;
using Diagnosis.App.Messaging;

namespace Diagnosis.App
{
    class DebugOutput
    {
        bool showPropertySelectedValueChanged = false;
        public static int GetSubscriberCount(EditableEventHandler eventHandler)
        {
            var count = 0;
            if (eventHandler != null)
            {
                count = eventHandler.GetInvocationList().Length;
            }
            Console.WriteLine("{0} has {1} subscribers", eventHandler, count);
            return count;
        }

        public DebugOutput()
        {
            this.Subscribe((int)EventID.WordCheckedChanged, (e) =>
            {
                var word = e.GetValue<WordViewModel>(Messages.Word);
                var isChecked = word.IsChecked;
                Console.WriteLine("word '{0}' {1}", word, isChecked ? "checked" : "unchecked");
            });
            this.Subscribe((int)EventID.DiagnosisCheckedChanged, (e) =>
            {
                var dia = e.GetValue<DiagnosisViewModel>(Messages.Diagnosis);
                var isChecked = dia.IsChecked;
                Console.WriteLine("dia '{0}' {1}", dia, isChecked ? "checked" : "unchecked");
            });
            this.Subscribe((int)EventID.CategoryCheckedChanged, (e) =>
            {
                var cat = e.GetValue<CategoryViewModel>(Messages.Category);
                var isChecked = cat.IsChecked;
                Console.WriteLine("cat '{0}' {1}", cat, isChecked ? "checked" : "unchecked");
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
                Console.WriteLine("cur patient is '{0}'", p);
            });
            this.Subscribe((int)EventID.CurrentDoctorChanged, (e) =>
            {
                var d = e.GetValue<DoctorViewModel>(Messages.Doctor);
                Console.WriteLine("cur doctor is '{0}'", d);
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

            this.Subscribe((int)EventID.WordsEditingModeChanged, (e) =>
            {
                var isEditing = e.GetValue<bool>(Messages.Boolean);
                Console.WriteLine("words editing is {0}", isEditing);
            });
            this.Subscribe((int)EventID.PropertySelectedValueChanged, (e) =>
            {
                var property = e.GetValue<PropertyViewModel>(Messages.Property);
                if (showPropertySelectedValueChanged)
                    Console.WriteLine("property {0} changed", property);
            });

            this.Subscribe((int)EventID.OpenHealthRecord, (e) =>
            {
                var hr = e.GetValue<HealthRecordViewModel>(Messages.HealthRecord);
                Console.WriteLine("open hr {0}", hr);
            });
            this.Subscribe((int)EventID.OpenSettings, (e) =>
            {
                var set = e.GetValue<SettingsViewModel>(Messages.Settings);
                Console.WriteLine("open setting {0}", set);
            });
        }
    }
}
