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
            return hr.Course ?? (hr.Appointment != null ? hr.Appointment.Course : null);
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
            {
                return (holder as Patient).GetAllHrs();
            }
            if (holder is Course)
            {
                return (holder as Course).GetAllHrs();
            }
            if (holder is Appointment)
            {
                return (holder as Appointment).HealthRecords;
            }

            throw new ArgumentOutOfRangeException("holder");
        }

        /// <summary>
        /// Все слова из записей держателя и его вложенных держателей. С повторами.
        /// </summary>
        /// <param name="holder"></param>
        /// <returns></returns>
        [Pure]
        public static IEnumerable<Word> GetAllWords(this IHrsHolder holder)
        {
            if (holder is Patient)
            {
                return (holder as Patient).GetAllWords();
            }
            if (holder is Course)
            {
                return (holder as Course).GetAllWords();
            }
            if (holder is Appointment)
            {
                return (holder as Appointment).GetAllWords();
            }

            throw new ArgumentOutOfRangeException("holder");
        }

        /// <summary>
        /// Удаляет пустые записи держателя.
        /// </summary>
        /// <param name="holder"></param>
        public static void DeleteEmptyHrs(this IHrsHolder holder)
        {
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
            {
                return holder as Patient;
            }
            if (holder is Course)
            {
                return (holder as Course).Patient;
            }
            if (holder is Appointment)
            {
                return (holder as Appointment).Course.Patient;
            }

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
            return pWords.Union(cWords);
        }
        [Pure]
        static IEnumerable<Word> GetAllWords(this Course course)
        {
            var cWords = course.HealthRecords.SelectMany(hr => hr.Words);
            var appsWords = course.Appointments.SelectMany(app => app.GetAllWords());
            return cWords.Union(appsWords);
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
                        return doc.Appointments.Count() == 0 && 
                            doc.Courses.Count() == 0 && 
                            doc.HealthRecords.All(h => h.IsEmpty());;
                    } 
                },
                { typeof(Patient), () => 
                    {
                        var pat = entity as Patient;
                        return pat.Courses.Count() == 0 && 
                            pat.HealthRecords.All(h => h.IsEmpty());
                    } 
                },
                { typeof(Course), () => 
                    {
                        var course = entity as Course;
                        return course.Appointments.Count() == 0 && 
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
                            hr.FromDay == null && 
                            hr.FromMonth == null && 
                            hr.FromYear == null && 
                            hr.HrItems.Count() == 0;
                    }
                },
                { typeof(Word),() =>
                    {
                        var w = entity as Word;
                        return w.HealthRecords.Count() == 0;
                    }
                },
                { typeof(Vocabulary),() =>
                    {
                        var w = entity as Vocabulary;
                        return w.Words.Count() == 0;
                    }
                },
            };

            var type = entity.Actual.GetType();
            if (@switch.Keys.Contains(type))
                return @switch[type]();

            throw new NotSupportedException();
        }

        /// <summary>
        /// Выражение для проверки равенства двух сущностей по значению, несмотря на разные ID.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> EqualsByVal<T>(T x) where T : IEntity
        {
            var @switch2 = new Dictionary<Type, Expression<Func<T, bool>>> {
                { typeof(Uom), (y) => 
                    (x as Uom).Abbr == (y as Uom).Abbr &&
                    (x as Uom).Type.Title == (y as Uom).Type.Title                   
                },
                { typeof(UomType), (y) => 
                    (x as UomType).Title == (y as UomType).Title
                },
                { typeof(HrCategory), (y) => 
                    (x as HrCategory).Title == (y as HrCategory).Title
                },
                { typeof(Speciality), (y) => 
                    (x as Speciality).Title == (y as Speciality).Title
                },
                { typeof(Vocabulary), (y) => 
                    (x as Vocabulary).Title == (y as Vocabulary).Title
                },
                { typeof(SpecialityIcdBlocks), (y) =>
                    (x as SpecialityIcdBlocks).IcdBlock == (y as SpecialityIcdBlocks).IcdBlock &&
                    (x as SpecialityIcdBlocks).Speciality.Title == (y as SpecialityIcdBlocks).Speciality.Title
                },
                { typeof(SpecialityVocabularies), (y) =>
                    (x as SpecialityVocabularies).Vocabulary.Title == (y as SpecialityVocabularies).Vocabulary.Title &&
                    (x as SpecialityVocabularies).Speciality.Title == (y as SpecialityVocabularies).Speciality.Title
                },
                //{ typeof(VocabularyWords), (y) => // не загружается с сервера
                //    (x as VocabularyWords).Vocabulary.Title == (y as VocabularyWords).Vocabulary.Title &&
                //    (x as VocabularyWords).Word.Title == (y as VocabularyWords).Word.Title
                //}
            };
            var type = x.Actual.GetType();

            if (@switch2.Keys.Contains(type))
                return @switch2[type];

            throw new NotImplementedException();
        }
        /// <summary>
        /// Проверяет равенство двух сущностей по значению, несмотря на разные ID.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool EqualsByVal<T>(this T x, T y) where T : IEntity
        {
            Contract.Requires(x.Actual.GetType() == y.Actual.GetType());
            var @switch = new Dictionary<Type, Func<bool>> {
                { typeof(Uom), () => 
                    {
                        var a = x as Uom;
                        var b = y as Uom;
                        return a.Abbr == b.Abbr &&
                               a.Type == b.Type;
                    }
                },
                { typeof(UomType), () => 
                    {
                         var a = x as UomType;
                        var b = y as UomType;
                        return a.Title == b.Title;
                    } 
                },
                { typeof(HrCategory), () => 
                    {
                        var a = x as HrCategory;
                        var b = y as HrCategory;
                        return a.Title == b.Title;
                    } 
                },
                { typeof(Speciality), () => 
                    {
                        var a = x as Speciality;
                        var b = y as Speciality;
                        return a.Title == b.Title;
                    } 
                },
                { typeof(Vocabulary), () => 
                    {
                        var a = x as Vocabulary;
                        var b = y as Vocabulary;
                        return a.Title == b.Title;
                    } 
                },
                { typeof(SpecialityIcdBlocks),() =>
                    {
                        var a = x as SpecialityIcdBlocks;
                        var b = y as SpecialityIcdBlocks;
                        return a.IcdBlock == b.IcdBlock &&
                               a.Speciality == b.Speciality;
                    }
                },
                { typeof(SpecialityVocabularies),() =>
                    {
                        var a = x as SpecialityVocabularies;
                        var b = y as SpecialityVocabularies;
                        return a.Vocabulary == b.Vocabulary &&
                               a.Speciality == b.Speciality;
                    }
                },
                //{ typeof(VocabularyWords),() =>
                //    {
                //        var a = x as VocabularyWords;
                //        var b = y as VocabularyWords;
                //        return a.Vocabulary == b.Vocabulary &&
                //               a.Word == b.Word;
                //    }
                //}
            };

            var type = x.Actual.GetType();

            if (@switch.Keys.Contains(type))
                return @switch[type]();

            throw new NotImplementedException();
        }

        /// <summary>
        /// Формат {[id] ToString()[,] ...}
        /// </summary>
        public static string FlattenString(this IEnumerable<IDomainObject> mayBeEntities)
        {
            var str = mayBeEntities.Select(item =>
            {
                var pre = "";
                if (item is ConfindenceHrItemObject)
                {
                    item = ((ConfindenceHrItemObject)item).HIO;
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
                        // Comment or Mesure
                    }

                    pre += " ";
                }
                return string.Format("{0}{1}", pre, item);
            });
            return string.Join(", ", str);
        }

    }
}