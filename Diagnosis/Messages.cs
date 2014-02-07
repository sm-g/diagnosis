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
    }

    class SymptomCheckedChangedParams
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

    enum EventID
    {
        SymptomCheckedChanged
    }
}
