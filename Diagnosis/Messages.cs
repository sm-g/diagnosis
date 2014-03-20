using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Diagnosis.ViewModels;

namespace Diagnosis
{
    /// <summary>
    /// Ключи событий для EventAggregator
    /// </summary>
    static class Messages
    {
        public const string Symptom = "symptom";
        public const string CheckedState = "checked";
        public const string Patient = "patientVM";
        public const string Diagnosis = "diagnosis";
        public const string Property = "propertyVM";

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

    enum EventID
    {
        SymptomCheckedChanged,
        CurrentPatientChanged,
        PatientCheckedChanged,
        DiagnosisCheckedChanged,
        PropertySelectedValueChanged
    }

    interface IEventParams
    {
        KeyValuePair<string, object>[] Params { get; }
    }
}
