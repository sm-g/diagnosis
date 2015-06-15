using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public static class HrExtensions
    {
        /// <summary>
        /// Пациент, к которому относится запись
        /// </summary>
        /// <param name="hr"></param>
        /// <returns></returns>
        [Pure]
        public static Patient GetPatient(this HealthRecord hr)
        {
            Contract.Requires(hr != null);
            return hr.Patient ?? (hr.Course != null ? hr.Course.Patient : hr.Appointment.Course.Patient);
        }

        /// <summary>
        /// Курс, к которому относится запись
        /// </summary>
        /// <param name="hr"></param>
        /// <returns></returns>
        [Pure]
        public static Course GetCourse(this HealthRecord hr)
        {
            Contract.Requires(hr != null);
            return hr.Course ?? (hr.Appointment != null ? hr.Appointment.Course : null);
        }

        [Pure]
        public static IEnumerable<IHrItemObject> GetOrderedEntities(this HealthRecord hr)
        {
            Contract.Requires(hr != null);
            return from item in hr.HrItems
                   orderby item.Ord
                   select item.Entity;
        }

        [Pure]
        public static IEnumerable<ConfWithHio> GetOrderedCHIOs(this HealthRecord hr)
        {
            Contract.Requires(hr != null);
            return from item in hr.HrItems
                   orderby item.Ord
                   select item.GetConfindenceHrItemObject();
        }

        [Pure]
        public static IEnumerable<ConfWithHio> GetCHIOs(this HealthRecord hr)
        {
            Contract.Requires(hr != null);
            return hr.HrItems.Select(x => x.GetConfindenceHrItemObject());
        }

        [Pure]
        public static IEnumerable<Confindencable<Word>> GetCWords(this HealthRecord hr)
        {
            Contract.Requires(hr != null);
            return hr.HrItems.Where(x => x.Word != null).Select(x => x.Word.AsConfidencable(x.Confidence));
        }

        [Pure]
        public static IEnumerable<Confindencable<Word>> GetCWordsNotFromMeasure(this HealthRecord hr)
        {
            Contract.Requires(hr != null);
            return hr.HrItems.Where(x => x.Entity is Word).Select(x => x.Word.AsConfidencable(x.Confidence));
        }
    }

    public static class IHrItemObjectExtensions
    {
        public static bool IsValid(this IHrItemObject hio)
        {
            Contract.Requires(hio != null);
            if (hio is Comment) return !(hio as Comment).String.IsNullOrEmpty();
            if (hio is IValidatable) return (hio as IValidatable).IsValid();
            return true;
        }

        public static ConfWithHio AsConfWithHio(this IHrItemObject hio, Confidence conf = Confidence.Present)
        {
            Contract.Requires(hio != null);
            return new ConfWithHio(hio, conf);
        }

        public static Confindencable<T> AsConfidencable<T>(this T hio, Confidence conf = Confidence.Present) where T : IHrItemObject
        {
            Contract.Requires(hio != null);
            return new Confindencable<T>(hio, conf);
        }

        public static MeasureOp ToMeasureOp(this Measure m, MeasureOperator op = MeasureOperator.GreaterOrEqual)
        {
            Contract.Requires(m != null);
            return new MeasureOp(op, m.Value, m.Uom, m.Word);
        }

        public static Measure AsMeasure(this MeasureOp mop)
        {
            Contract.Requires(mop != null);
            return new Measure(mop.Value, mop.Uom, mop.Word);
        }

        public static ConfWithHio GetConfindenceHrItemObject(this HrItem hi)
        {
            Contract.Requires(hi != null);
            return new ConfWithHio(hi.Entity, hi.Confidence);
        }
    }

    public static class SearchOptionsExtensions
    {
        public static IEnumerable<Word> GetAllWords(this SearchOptions o)
        {
            Contract.Requires(o != null);
            return o.Children.Aggregate(o
                .WordsAll
                .Union(o.WordsAny)
                .Union(o.WordsNot)
                .Union(o.MeasuresAll.Select(x => x.Word))
                .Union(o.MeasuresAny.Select(x => x.Word)), (acc, opt) => acc.Union(opt.GetAllWords()));
        }
    }

    public static class ICritExtensions
    {
        [Pure]
        public static Estimator GetEstimator(this ICrit crit)
        {
            Contract.Requires(crit != null);

            if (crit is Estimator)
                return crit as Estimator;
            if (crit is CriteriaGroup)
                return (crit as CriteriaGroup).Estimator;
            if (crit is Criterion)
                return (crit as Criterion).Group == null ? null : (crit as Criterion).Group.Estimator;

            throw new NotImplementedException();
        }
    }

    public static class IHrsHolderExtensions
    {
        /// <summary>
        /// Все записи держателя и его вложенных держателей.
        /// </summary>
        [Pure]
        public static IEnumerable<HealthRecord> GetAllHrs(this IHrsHolder holder)
        {
            Contract.Requires(holder != null);

            if (holder is Patient)
                return (holder as Patient).GetAllHrs();
            if (holder is Course)
                return (holder as Course).GetAllHrs();
            if (holder is Appointment)
                return (holder as Appointment).HealthRecords;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Все слова из записей держателя и его вложенных держателей. С повторами. Со словами измерений.
        /// </summary>
        /// <param name="holder"></param>
        /// <returns></returns>
        [Pure]
        public static IEnumerable<Word> GetAllWords(this IHrsHolder holder)
        {
            if (holder is Patient)
                return (holder as Patient).GetAllWords();
            if (holder is Course)
                return (holder as Course).GetAllWords();
            if (holder is Appointment)
                return (holder as Appointment).GetAllWords();

            throw new NotImplementedException();
        }

        /// <summary>
        /// Дата последнего обновления записей внутри.
        /// Дата обновления, если запсией нет.
        /// </summary>
        public static DateTime GetLastHrUpdatedAt(this IHrsHolder holder)
        {
            Contract.Requires(holder != null);

            if (holder.GetAllHrs().Any())
                return holder.GetAllHrs()
                    .OrderByDescending(x => x.UpdatedAt)
                    .First().UpdatedAt;
            else
                return (holder as IHaveAuditInformation).UpdatedAt;
        }

        /// <summary>
        /// Пациент, к которому относится держатель.
        /// </summary>
        /// <param name="holder"></param>
        [Pure]
        public static Patient GetPatient(this IHrsHolder holder)
        {
            if (holder is Patient)
                return holder as Patient;
            if (holder is Course)
                return (holder as Course).Patient;
            if (holder is Appointment)
                return (holder as Appointment).Course.Patient;

            throw new ArgumentOutOfRangeException("holder");
        }

        [Pure]
        private static IEnumerable<HealthRecord> GetAllHrs(this Patient pat)
        {
            var courseHrs = pat.Courses.SelectMany(x => x.GetAllHrs());
            return pat.HealthRecords.Union(courseHrs);
        }

        [Pure]
        private static IEnumerable<HealthRecord> GetAllHrs(this Course course)
        {
            var appHrs = course.Appointments.SelectMany(x => x.HealthRecords);
            return course.HealthRecords.Union(appHrs);
        }

        [Pure]
        private static IEnumerable<Word> GetAllWords(this Patient patient)
        {
            var pWords = patient.HealthRecords.SelectMany(hr => hr.Words);
            var cWords = patient.Courses.SelectMany(c => c.GetAllWords());
            return pWords.Concat(cWords);
        }

        [Pure]
        private static IEnumerable<Word> GetAllWords(this Course course)
        {
            var cWords = course.HealthRecords.SelectMany(hr => hr.Words);
            var appsWords = course.Appointments.SelectMany(app => app.GetAllWords());
            return cWords.Concat(appsWords);
        }

        [Pure]
        private static IEnumerable<Word> GetAllWords(this Appointment app)
        {
            return app.HealthRecords.SelectMany(hr => hr.Words);
        }
    }

    public static class IEntityExtensions
    {
        /// <summary>
        /// Формат {[id] ToString()[,] ...}
        /// </summary>
        public static string FlattenString(this IEnumerable<IDomainObject> mayBeEntities)
        {
            Contract.Requires(mayBeEntities != null);

            var str = mayBeEntities.Select(item =>
            {
                var pre = "";
                if (item is ConfWithHio)
                {
                    item = ((ConfWithHio)item).HIO;
                }
                if (item is IHrItemObject)
                {
                    dynamic entity = item;
                    try
                    {
                        if (entity.Id is Guid)
                            pre = string.Format("#{0}..", entity.Id.ToString().Substring(0, 3));
                        else
                            pre = string.Format("#{0}", entity.Id);
                    }
                    catch
                    {
                        // Comment or Measure
                    }

                    pre += " ";
                }
                return string.Format("{0}{1}", pre, item);
            });
            return string.Join(", ", str);
        }
    }
}