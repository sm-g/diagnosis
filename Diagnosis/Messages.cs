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
        public const string Patient = "patient";
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

    enum EventID
    {
        SymptomCheckedChanged,
        CurrentPatientChanged
    }

    interface IEventParams
    {
        KeyValuePair<string, object>[] Params { get; }
    }
}
