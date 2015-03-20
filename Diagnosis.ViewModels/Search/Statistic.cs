using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Search
{
    public class Statistic
    {

        public Statistic(IEnumerable<HealthRecord> hrs)
        {
            HealthRecords = hrs
                .OrderBy(p => p.CreatedAt)
                .ToList()
                .AsReadOnly();

            Patients = hrs
                .Select(x => x.GetPatient())
                .Distinct()
                .OrderBy(p => p.FullName)
                .ToList()
                .AsReadOnly();

            Words = hrs
                .SelectMany(x => x.Words)
                .Distinct()
                .OrderBy(x => x.Title)
                .ToList()
                .AsReadOnly();

            WordsWithMeasure = hrs
                .SelectMany(x => x.Measures)
                .Select(x => x.Word)
                .Distinct()
                .OrderBy(x => x.Title)
                .ToList()
                .AsReadOnly();

            GridValues = new Dictionary<HealthRecord, Dictionary<Word, GridValue>>();
            foreach (var hr in HealthRecords)
            {
                foreach (var word in Words)
                {
                    var values = ItemsWithWord(word, hr).SelectMany(item => ValuesFor(item));
                    // обычно в записи только один элемент с данным словом.
                    // если больше, выводим наиболее важное значение, порядок - числа, наличие слова, отрицание

                    GridValue val;
                    if (values.OfType<Measure>().Any())
                        val = new GridValue(values.OfType<Measure>());
                    else if (values.Any(x => x.Equals(true)))
                        val = new GridValue(true);
                    else if (values.Any(x => x.Equals(false)))
                        val = new GridValue(false);
                    else
                        val = new GridValue();

                    if (!GridValues.Keys.Contains(hr))
                        GridValues[hr] = new Dictionary<Word, GridValue>();
                    GridValues[hr][word] = val;
                }
            }
        }

        /// <summary>
        /// Пациенты, о которых записи, по имени
        /// </summary>
        public ReadOnlyCollection<Patient> Patients { get; private set; }

        /// <summary>
        /// Все слова из записей, по алфавиту
        /// </summary>
        public ReadOnlyCollection<Word> Words { get; private set; }

        /// <summary>
        /// Слова для которых есть измерение, по алфавиту
        /// </summary>
        public ReadOnlyCollection<Word> WordsWithMeasure { get; private set; }

        /// <summary>
        /// Записи, по дате создания
        /// </summary>
        public ReadOnlyCollection<HealthRecord> HealthRecords { get; private set; }

        public Dictionary<HealthRecord, Dictionary<Word, GridValue>> GridValues { get; private set; }

        public int PatientsCount
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
            get { return PatientsCount == 0 ? -1 : Patients.Where(p => p.Age.HasValue).Max(p => p.Age); }
        }

        public int? MinAge
        {
            get { return PatientsCount == 0 ? -1 : Patients.Where(p => p.Age.HasValue).Min(p => p.Age); }
        }

        internal int MaxMeasuresFor(Word word)
        {
            return GridValues.Values.Max(y => y[word].Measures != null ? y[word].Measures.Count() : 0);
        }

        internal bool HasMeasuresFor(Word word)
        {
            return GridValues.Values.Any(y => y[word].Measures != null);
        }

        private IEnumerable<object> ValuesFor(HrItem item)
        {
            if (item.Measure != null)
            {
                yield return item.Measure;
            }
            else if (item.Confidence == Models.Confidence.Present)
            {
                yield return true;
            }
            else if (item.Confidence == Models.Confidence.Absent)
            {
                yield return false;
            }
        }
        private IEnumerable<HrItem> ItemsWithWord(Word w, HealthRecord hr)
        {
            return hr.HrItems.Where(x => x.Word == w);
        }
        /// <summary>
        /// Значение в ячейке таблицы — true/false/несколько измерений.
        /// </summary>
        public class GridValue
        {
            public GridValue(IEnumerable<Measure> measures)
            {
                Contract.Requires(measures != null);
                Contract.Requires(measures.Count() > 0);
                Measures = measures;
            }

            public GridValue(bool boolean)
            {
                Bool = boolean;
            }

            public GridValue()
            {
                // null value
            }

            public bool? Bool { get; private set; }

            public IEnumerable<Measure> Measures { get; private set; }
        }
    }
}