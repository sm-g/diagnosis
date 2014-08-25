using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventAggregator;
using Diagnosis.ViewModels;
using Diagnosis.Models;
using Diagnosis.App.Messaging;
using System.Diagnostics;
using System.Threading;
using Diagnosis.Core;

namespace Diagnosis.App
{
    public class DebugOutput
    {
        bool showPropertySelectedValueChanged = false;

        public static int GetSubscriberCount(EditableEventHandler eventHandler)
        {
            var count = 0;
            if (eventHandler != null)
            {
                count = eventHandler.GetInvocationList().Length;
            }
            Debug.Print("{0} has {1} subscribers", eventHandler, count);
            return count;
        }

        public void PrintMemoryUsage()
        {
            while (true)
            {
                Debug.Print("total memory {0}", GC.GetTotalMemory(true));
                Thread.Sleep(5000);
            }
        }

        public DebugOutput()
        {
            var debugThread = new Thread(PrintMemoryUsage) { IsBackground = true };
            //  debugThread.Start();

            this.Subscribe((int)EventID.WordCheckedChanged, (e) =>
            {
                var word = e.GetValue<WordViewModel>(Messages.Word);
                var isChecked = word.IsChecked;
                Debug.Print("word '{0}' {1}", word, isChecked ? "checked" : "unchecked");
            });
            this.Subscribe((int)EventID.DiagnosisCheckedChanged, (e) =>
            {
                var dia = e.GetValue<DiagnosisViewModel>(Messages.Diagnosis);
                var isChecked = dia.IsChecked;
                Debug.Print("dia '{0}' {1}", dia, isChecked ? "checked" : "unchecked");
            });
            this.Subscribe((int)EventID.CategoryCheckedChanged, (e) =>
            {
                var cat = e.GetValue<CategoryViewModel>(Messages.Category);
                var isChecked = cat.IsChecked;
                Debug.Print("cat '{0}' {1}", cat, isChecked ? "checked" : "unchecked");
            });

            this.Subscribe((int)EventID.AppointmentAdded, (e) =>
            {
                var app = e.GetValue<AppointmentViewModel>(Messages.Appointment);
                Debug.Print("app {0} added", app);
            });
            this.Subscribe((int)EventID.CourseStarted, (e) =>
            {
                var c = e.GetValue<Course>(Messages.Course);
                Debug.Print("course {0} started", c);
            });

            this.Subscribe((int)EventID.OpenedPatientChanged, (e) =>
            {
                var p = e.GetValue<PatientViewModel>(Messages.Patient);
                Debug.Print("current patient is '{0}'", p);
            });
            this.Subscribe((int)EventID.CurrentDoctorChanged, (e) =>
            {
                var d = e.GetValue<Doctor>(Messages.Doctor);
                Debug.Print("current doctor is '{0}'", d);
            });

            this.Subscribe((int)EventID.HealthRecordChanged, (e) =>
            {
                var hr = e.GetValue<HealthRecordViewModel>(Messages.HealthRecord);
                Debug.Print("hr {0} changed", hr);
            });
            this.Subscribe((int)EventID.HealthRecordSelected, (e) =>
            {
                var hr = e.GetValue<HealthRecordViewModel>(Messages.HealthRecord);
                Debug.Print("hr {0} selected", hr);
            });

            this.Subscribe((int)EventID.WordsEditingModeChanged, (e) =>
            {
                var isEditing = e.GetValue<bool>(Messages.Boolean);
                Debug.Print("words editing is {0}", isEditing);
            });
            this.Subscribe((int)EventID.PropertySelectedValueChanged, (e) =>
            {
                var property = e.GetValue<PropertyViewModel>(Messages.Property);
                if (showPropertySelectedValueChanged)
                    Debug.Print("property {0} changed", property);
            });

            this.Subscribe((int)EventID.OpenHealthRecord, (e) =>
            {
                var hr = e.GetValue<HealthRecord>(Messages.HealthRecord);
                Debug.Print("open hr {0}", hr);
            });
        }
    }
}
