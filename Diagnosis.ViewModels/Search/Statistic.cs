using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Search
{
    public class Statistic
    {
        Dictionary<Patient, List<Word>> patientWords = new Dictionary<Patient, List<Word>>();

        public Statistic(IEnumerable<Patient> ps)
        {
            Patients = ps.ToList().AsReadOnly();
            Words = Patients.SelectMany(p => p.GetAllWords()).Distinct().ToList().AsReadOnly();

            PatientHasWord = new bool[Patients.Count, Words.Count];
            for (int p = 0; p < Patients.Count; p++)
            {
                var allWords = Patients[p].GetAllWords().ToList();
                patientWords[Patients[p]] = allWords;

                for (int w = 0; w < Words.Count; w++)
                {
                    PatientHasWord[p, w] = patientWords[Patients[p]].Contains(Words[w]);
                }
            }
        }
        public ReadOnlyCollection<Patient> Patients { get; private set; }

        public ReadOnlyCollection<Word> Words { get; private set; }

        public bool[,] PatientHasWord { get; private set; }

        public int Count
        {
            get { return Patients.Count; }
        }
        public int Females
        {
            get { return Patients.Where(p => p.IsMale.HasValue && !p.IsMale.Value).Count(); }
        }
        public int Males
        {
            get { return Patients.Where(p => p.IsMale.HasValue && p.IsMale.Value).Count(); }
        }
        public int UnknownSex
        {
            get { return Patients.Where(p => !p.IsMale.HasValue).Count(); }
        }

        public int? MaxAge
        {
            get { return Count == 0 ? -1 : Patients.Where(p => p.Age.HasValue).Max(p => p.Age); }
        }

        public int? MinAge
        {
            get { return Count == 0 ? -1 : Patients.Where(p => p.Age.HasValue).Min(p => p.Age); }
        }

        public bool this[Patient p, Word w]
        {
            get { return patientWords[p].Contains(w); }
        }
    }
}
