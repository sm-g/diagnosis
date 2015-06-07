using Diagnosis.Models;
using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public abstract class StatisticBase : IDisposable
    {        /// <summary>
        /// Пациенты, о которых записи, по имени
        /// </summary>
        public abstract ReadOnlyCollection<Patient> Patients { get; }

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

        public abstract void Dispose();
    }

    public class CritStatistic : StatisticBase
    {
        private ReadOnlyCollection<Patient> _pats;
        private Dictionary<Patient, IEnumerable<Criterion>> patCrs;

        public CritStatistic(Dictionary<Patient, IEnumerable<Criterion>> patCrs)
        {
            this.patCrs = patCrs;
            _pats = patCrs.Keys.ToList().AsReadOnly();
        }

        public override ReadOnlyCollection<Patient> Patients
        {
            get { return _pats; }
        }

        public override void Dispose()
        {
        }
    }

    public class HrsStatistic : StatisticBase
    {
        private ReadOnlyCollection<Patient> _pats;
        private bool initialized;

        public HrsStatistic(IEnumerable<HealthRecord> hrs)
        {
            HealthRecords = hrs
                .OrderBy(p => p.CreatedAt)
                .ToList()
                .AsReadOnly();

            _pats = hrs
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

            Icds = hrs
                .SelectMany(x => x.Diseases)
                .Distinct()
                .OrderBy(x => x.Code)
                .ToList()
                .AsReadOnly();

            WordsWithMeasure = hrs
                .SelectMany(x => x.Measures)
                .Select(x => x.Word)
                .Distinct()
                .OrderBy(x => x.Title)
                .ToList()
                .AsReadOnly();

            GridValues = new Dictionary<HealthRecord, Dictionary<IHrItemObject, GridValue>>();

            FillGridValues();
            initialized = true;
        }

        /// <summary>
        /// Все слова из записей, по алфавиту
        /// </summary>
        public ReadOnlyCollection<Word> Words { get; private set; }

        /// <summary>
        /// Все диагнозы из записей, по коду
        /// </summary>
        public ReadOnlyCollection<IcdDisease> Icds { get; private set; }

        /// <summary>
        /// Слова для которых есть измерение, по алфавиту
        /// </summary>
        public ReadOnlyCollection<Word> WordsWithMeasure { get; private set; }

        /// <summary>
        /// Записи, по дате создания
        /// </summary>
        public ReadOnlyCollection<HealthRecord> HealthRecords { get; private set; }

        public Dictionary<HealthRecord, Dictionary<IHrItemObject, GridValue>> GridValues { get; private set; }

        public override ReadOnlyCollection<Patient> Patients
        {
            get { return _pats; }
        }

        private void FillGridValues()
        {
            foreach (var hr in HealthRecords)
            {
                foreach (var hio in Icds.Union<IHrItemObject>(Words))
                {
                    var values = ItemsWith(hio, hr)
                        .Select(item => ValueOf(item))
                        .Where(x => x != null)
                        .ToList();

                    // обычно в записи только один элемент с данным словом
                    // если больше, выводим наиболее важное значение, порядок - числа, наличие слова, отрицание

                    GridValue val;
                    if (values.OfType<Measure>().Any())
                    {
                        Contract.Assume(hio is Word);
                        val = new GridValue(values.OfType<Measure>());
                    }
                    else if (values.Any(x => x.Equals(true)))
                        val = new GridValue(true);
                    else if (values.Any(x => x.Equals(false)))
                        val = new GridValue(false);
                    else
                        val = new GridValue();

                    if (!GridValues.Keys.Contains(hr))
                        GridValues[hr] = new Dictionary<IHrItemObject, GridValue>();
                    GridValues[hr][hio] = val;
                }
            }
        }

        /// <summary>
        /// Значение элемента, по приоритету - числа, наличие, отрицание.
        /// </summary>
        private object ValueOf(HrItem item)
        {
            if (item.Measure != null)
                return item.Measure;
            else if (item.Confidence == Models.Confidence.Present)
                return true;
            else if (item.Confidence == Models.Confidence.Absent)
                return false;
            return null;
        }

        /// <summary>
        /// Элементы записи со словом / диагнозом.
        /// </summary>
        private IEnumerable<HrItem> ItemsWith(IHrItemObject hio, HealthRecord hr)
        {
            Contract.Requires(hio is Word || hio is IcdDisease);

            var w = hio as Word;
            var icd = hio as IcdDisease;
            if (w != null)
                return hr.HrItems.Where(x => x.Word == w);
            else if (icd != null)
                return hr.HrItems.Where(x => x.Disease == icd);
            return Enumerable.Empty<HrItem>();
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(GridValues.Values.All(x => x.Keys.All(hio => hio is Word || hio is IcdDisease)));
            Contract.Invariant(!initialized || GridValues.Keys.Count == HealthRecords.Count);
            Contract.Invariant(!initialized || GridValues.Values.All(x => x.Keys.Count == Words.Count + Icds.Count));
        }

        /// <summary>
        /// Значение в ячейке таблицы — true/false/несколько измерений/ничего.
        /// </summary>
        public class GridValue
        {
            private readonly IEnumerable<Measure> measures = Enumerable.Empty<Measure>();
            private readonly bool? boolean;
            public GridValue(IEnumerable<Measure> measures)
            {
                Contract.Requires(measures != null);
                Contract.Requires(measures.Any());
                this.measures = measures.ToArray();
            }

            public GridValue(bool boolean)
            {
                this.boolean = boolean;
            }

            public GridValue()
            {
                // null value
            }

            public bool? Bool { get { return boolean; } }

            public IEnumerable<Measure> Measures { get { return measures; } }
        }

        public override void Dispose()
        {
            if (GridValues != null)
                GridValues.Clear();
            GridValues = null;
        }
    }
}