﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Word
    {
        ISet<Symptom> symptoms = new HashSet<Symptom>();
        ISet<SymptomWords> symptomWords = new HashSet<SymptomWords>();

        public virtual int Id { get; protected set; }
        public virtual string Title { get; set; }
        public virtual byte Priority { get; set; }
        public virtual bool IsEnum { get; set; }
        public virtual Category DefaultCategory { get; set; }
        public virtual Word Parent { get; set; }
        public virtual ReadOnlyCollection<Symptom> Symptoms
        {
            get
            {
                return new ReadOnlyCollection<Symptom>(
                    new List<Symptom>(symptoms));
            }
        }
        public virtual ReadOnlyCollection<SymptomWords> SymptomWords
        {
            get
            {
                return new ReadOnlyCollection<SymptomWords>(
                    new List<SymptomWords>(symptomWords));
            }
        }

        public Word(string title, byte priority = 0)
        {
            Contract.Requires(!string.IsNullOrEmpty(title));
            Title = title;
            Priority = priority;
        }

        protected Word() { }
    }


    public class CompareWord : IComparer<Word>
    {
        public int Compare(Word x, Word y)
        {
            if (x.Priority != y.Priority)
            {
                return x.Priority.CompareTo(y.Priority);
            }
            else
            {
                return x.Title.CompareTo(y.Title);
            }
        }

    }
}
