using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Diagnosis.ViewModels;
using Diagnosis.Models;

namespace Diagnosis
{
    /// <summary>
    /// Ключи событий для EventAggregator
    /// </summary>
    static class Messages
    {
        public const string Symptom = "symptomVM";
        public const string CheckedState = "checked";
        public const string Patient = "patientVM";
        public const string Diagnosis = "diagnosisVM";
        public const string Property = "propertyVM";
        public const string Course = "course";

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
            Contract.Requires(patient != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Patient, patient)
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

    class CourseStartesParams : IEventParams
    {
        public KeyValuePair<string, object>[] Params { get; private set; }

        public CourseStartesParams(Course course)
        {
            Contract.Requires(course != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Course, course)
            };
        }
    }

    enum EventID
    {
        SymptomCheckedChanged,
        CurrentPatientChanged,
        PatientCheckedChanged,
        DiagnosisCheckedChanged,
        PropertySelectedValueChanged,
        CourseStarted
    }

    interface IEventParams
    {
        KeyValuePair<string, object>[] Params { get; }
    }
}
