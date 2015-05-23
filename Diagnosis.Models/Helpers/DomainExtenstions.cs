using System;
using System.Collections.Generic;
using System.Linq;
using Diagnosis.Common;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

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
            return o.Children.Aggregate(o.WordsAll.Union(o.WordsAny).Union(o.WordsNot), (acc, opt) => acc.Union(opt.GetAllWords()));
        }
    }

    public static class ICritExtensions
    {
        [Pure]
        public static Estimator GetEstimator(this ICrit crit)
        {
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
        /// Удаляет пустые записи держателя.
        /// </summary>
        /// <param name="holder"></param>
        public static void DeleteEmptyHrs(this IHrsHolder holder)
        {
            Contract.Requires(holder != null);
            var emptyHrs = holder.HealthRecords.Where(hr => hr.IsEmpty()).ToList();
            emptyHrs.ForEach(hr => holder.RemoveHealthRecord(hr));
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
        static IEnumerable<HealthRecord> GetAllHrs(this Patient pat)
        {
            var courseHrs = pat.Courses.SelectMany(x => x.GetAllHrs());
            return pat.HealthRecords.Union(courseHrs);
        }
        [Pure]
        static IEnumerable<HealthRecord> GetAllHrs(this Course course)
        {
            var appHrs = course.Appointments.SelectMany(x => x.HealthRecords);
            return course.HealthRecords.Union(appHrs);
        }
        [Pure]
        static IEnumerable<Word> GetAllWords(this Patient patient)
        {
            var pWords = patient.HealthRecords.SelectMany(hr => hr.Words);
            var cWords = patient.Courses.SelectMany(c => c.GetAllWords());
            return pWords.Concat(cWords);
        }
        [Pure]
        static IEnumerable<Word> GetAllWords(this Course course)
        {
            var cWords = course.HealthRecords.SelectMany(hr => hr.Words);
            var appsWords = course.Appointments.SelectMany(app => app.GetAllWords());
            return cWords.Concat(appsWords);
        }
        [Pure]
        static IEnumerable<Word> GetAllWords(this Appointment app)
        {
            return app.HealthRecords.SelectMany(hr => hr.Words);
        }
    }

    public static class IEntityExtensions
    {
        /// <summary>
        /// Определяет, пуста ли сущность.
        /// доктор — без курсов и осмотров
        /// пациент  — без записей и курсов
        /// курс — без записей и осмотров
        /// осмотр — без записей
        /// запись — без элементов и даты или удаленная
        /// слово — без записей с этим словом
        /// словарь — без слов
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [Pure]
        public static bool IsEmpty(this IEntity entity)
        {
            var @switch = new Dictionary<Type, Func<bool>> {
                { typeof(Doctor), () => 
                    {
                        var doc = entity as Doctor;
                        return !doc.Appointments.Any() && 
                            !doc.Courses.Any() && 
                            doc.HealthRecords.All(h => h.IsEmpty());;
                    } 
                },
                { typeof(Patient), () => 
                    {
                        var pat = entity as Patient;
                        return !pat.Courses.Any() && 
                            pat.HealthRecords.All(h => h.IsEmpty());
                    } 
                },
                { typeof(Course), () => 
                    {
                        var course = entity as Course;
                        return !course.Appointments.Any() && 
                            course.HealthRecords.All(h => h.IsEmpty());
                    } 
                },
                { typeof(Appointment), () => 
                    {
                        var app = entity as Appointment;
                        return app.HealthRecords.All(x => x.IsEmpty());
                    } 
                },
                { typeof(HealthRecord),() =>
                    {
                        var hr = entity as HealthRecord;
                        return hr.IsDeleted ||
                            hr.FromDate.IsEmpty && 
                            !hr.HrItems.Any();
                    }
                },
                { typeof(Word),() =>
                    {
                        var w = entity as Word;
                        return !w.HealthRecords.Any();
                    }
                },
                { typeof(Vocabulary),() =>
                    {
                        var w = entity as Vocabulary;
                        return !w.Words.Any();
                    }
                },
                { typeof(Estimator),() =>
                    {
                        var e = entity as Estimator;
                        return !e.CriteriaGroups.Any();
                    }
                },
                { typeof(CriteriaGroup),() =>
                    {
                        var w = entity as CriteriaGroup;
                        return !w.Criteria.Any();
                    }
                },
                { typeof(Criterion),() =>
                    {
                        var w = entity as Criterion;
                        return true;
                    }
                }
            };

            var type = entity.Actual.GetType();
            if (@switch.Keys.Contains(type))
                return @switch[type]();

            throw new NotSupportedException();
        }

        /// <summary>
        /// Формат {[id] ToString()[,] ...}
        /// </summary>
        public static string FlattenString(this IEnumerable<object> mayBeEntities)
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

    public static class DateOffsetExtensions
    {
        /// <summary>
        /// Округляет смещение.
        /// При укрупнении единицы смещение считается для полной даты с 1 вместо отсутствующих значений.
        /// </summary>
        public static int? RoundOffsetFor(this DateOffset d, DateUnit unit)
        {
            Contract.Requires(d != null);
            Contract.Ensures(d.Equals(Contract.OldValue(d)));
            Contract.Ensures(!d.IsEmpty || Contract.Result<int?>() == null);

            if (!d.Year.HasValue)
            {
                return null;
            }
            int? roundedOffset;
            switch (unit)
            {
                case DateUnit.Day:
                    roundedOffset = (d.Now - d.GetSortingDate()).Days;
                    break;

                case DateUnit.Week:
                    roundedOffset = (d.Now - d.GetSortingDate()).Days / 7;
                    break;

                case DateUnit.Month:
                    if (d.Month.HasValue)
                    {
                        roundedOffset = DateHelper.GetTotalMonthsBetween(d.Now, d.Year.Value, d.Month.Value);
                    }
                    else
                    {
                        roundedOffset = DateHelper.GetTotalMonthsBetween(d.Now, d.Year.Value, 1);
                    }
                    break;

                case DateUnit.Year:
                    roundedOffset = d.Now.Year - d.Year.Value;
                    break;

                default:
                    throw new NotImplementedException();
            }
            return roundedOffset;
        }
        /// <summary>
        /// Установка даты меняет единицу измерения и смещение на наиболее подходящие.
        /// </summary>
        public static DateOffset RoundOffsetUnitByDate(this DateOffset d, DateTime described)
        {
            Contract.Requires(d.Year != null);
            Contract.Ensures(d.Equals(Contract.OldValue(d)));

            int? offset = null;
            DateUnit unit = 0;

            Action setRoundedOffsetUnitMonthOrYear = () =>
            {
                var months = DateHelper.GetTotalMonthsBetween(described, d.Year.Value, d.Month.Value);
                if (months < 12) // меньше года - месяцы
                {
                    offset = months;
                    unit = DateUnit.Month;
                }
                else
                {
                    offset = described.Year - d.Year.Value;
                    unit = DateUnit.Year;
                }
            };

            if (d.Month == null) // _ _ y (или d _ y без автообрезания)
            {
                offset = described.Year - d.Year.Value;
                unit = DateUnit.Year;
            }
            else if (d.Day == null) // _ m y
            {
                setRoundedOffsetUnitMonthOrYear();
            }
            else // d m y
            {
                var days = (described - (DateTime)d).Days;
                if (days < 7) // меньше недели - дни
                {
                    offset = days;
                    unit = DateUnit.Day;
                }
                else if (days < 4 * 7) // меньше месяца - недели
                {
                    offset = days / 7;
                    unit = DateUnit.Week;
                }
                else
                {
                    setRoundedOffsetUnitMonthOrYear();
                }
            }

            return new DateOffset(offset, unit, () => d.Now);
        }
    }
}