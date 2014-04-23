using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public class SearchWrap
    {
        public readonly object entity;

        public string Name { get; private set; }
        public string Category { get; private set; }

        public SearchWrap(WordViewModel word)
        {
            Contract.Requires(word != null);

            entity = word;
            Name = word.Name;
            Category = word.DefaultCategory != null ? word.DefaultCategory.ToString() : "";
        }

        public SearchWrap(SymptomViewModel symptom)
        {
            Contract.Requires(symptom != null);

            entity = symptom;
            Name = symptom.Name;
            Category = symptom.DefaultCategory != null ? symptom.DefaultCategory.ToString() : "";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
