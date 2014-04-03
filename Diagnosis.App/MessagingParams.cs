using Diagnosis.Models;
using Diagnosis.App.ViewModels;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows;

namespace Diagnosis.App
{
    interface IEventParams
    {
        KeyValuePair<string, object>[] Params { get; }
    }

    class SymptomCheckedChangedParams : IEventParams
    {
        public KeyValuePair<string, object>[] Params { get; private set; }

        public SymptomCheckedChangedParams(SymptomViewModel symtpom, bool isChecked)
        {
            Contract.Requires(symtpom != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Symptom, symtpom),
                new KeyValuePair<string,object>(Messages.CheckedState, isChecked)
            };
        }
    }

    class DiagnosisCheckedChangedParams : IEventParams
    {
        public KeyValuePair<string, object>[] Params { get; private set; }

        public DiagnosisCheckedChangedParams(DiagnosisViewModel diagnosis, bool isChecked)
        {
            Contract.Requires(diagnosis != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Diagnosis, diagnosis),
                new KeyValuePair<string,object>(Messages.CheckedState, isChecked)
            };
        }
    }

    class PatientCheckedChangedParams : IEventParams
    {
        public KeyValuePair<string, object>[] Params { get; private set; }

        public PatientCheckedChangedParams(PatientViewModel patient, bool isChecked)
        {
            Contract.Requires(patient != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Patient, patient),
                new KeyValuePair<string,object>(Messages.CheckedState, isChecked)
            };
        }
    }

    class CurrentPatientChangedParams : IEventParams
    {
        public KeyValuePair<string, object>[] Params { get; private set; }

        public CurrentPatientChangedParams(PatientViewModel patient)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.Patient, patient)
            };
        }
    }

    class CurrentDoctorChangedParams : IEventParams
    {
        public KeyValuePair<string, object>[] Params { get; private set; }

        public CurrentDoctorChangedParams(DoctorViewModel doctor)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.Doctor, doctor)
            };
        }
    }

    class PropertySelectedValueChangedParams : IEventParams
    {
        public KeyValuePair<string, object>[] Params { get; private set; }

        public PropertySelectedValueChangedParams(PropertyViewModel property)
        {
            Contract.Requires(property != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Property, property)
            };
        }
    }

    class CourseStartedParams : IEventParams
    {
        public KeyValuePair<string, object>[] Params { get; private set; }

        public CourseStartedParams(Course course)
        {
            Contract.Requires(course != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Course, course)
            };
        }
    }

    class AppointmentAddedParams : IEventParams
    {
        public KeyValuePair<string, object>[] Params { get; private set; }

        public AppointmentAddedParams(Appointment appointment)
        {
            Contract.Requires(appointment != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Appointment, appointment)
            };
        }
    }

    class HealthRecordSelectedParams : IEventParams
    {
        public KeyValuePair<string, object>[] Params { get; private set; }

        public HealthRecordSelectedParams(HealthRecordViewModel hrVM)
        {
            Contract.Requires(hrVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.HealthRecord, hrVM)
            };
        }
    }

    class DirectoryEditingModeChangedParams : IEventParams
    {
        public KeyValuePair<string, object>[] Params { get; private set; }

        public DirectoryEditingModeChangedParams(bool isEditing)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.Boolean, isEditing)
            };
        }
    }
}
