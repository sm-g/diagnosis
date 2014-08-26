using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventAggregator;
using Diagnosis.ViewModels;
using Diagnosis.Models;
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

            this.Subscribe(Events.OpenedPatientChanged, (e) =>
            {
                var p = e.GetValue<PatientViewModel>(MessageKeys.Patient);
                Debug.Print("current patient is '{0}'", p);
            });
            this.Subscribe(Events.CurrentDoctorChanged, (e) =>
            {
                var d = e.GetValue<Doctor>(MessageKeys.Doctor);
                Debug.Print("current doctor is '{0}'", d);
            });

            this.Subscribe(Events.WordsEditingModeChanged, (e) =>
            {
                var isEditing = e.GetValue<bool>(MessageKeys.Boolean);
                Debug.Print("words editing is {0}", isEditing);
            });
            this.Subscribe(Events.PropertySelectedValueChanged, (e) =>
            {
                var property = e.GetValue<PropertyViewModel>(MessageKeys.Property);
                if (showPropertySelectedValueChanged)
                    Debug.Print("property {0} changed", property);
            });

            this.Subscribe(Events.OpenHealthRecord, (e) =>
            {
                var hr = e.GetValue<HealthRecord>(MessageKeys.HealthRecord);
                Debug.Print("open hr {0}", hr);
            });
        }
    }
}
