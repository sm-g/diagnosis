using System;
using System.Collections.Generic;
using System.Linq;
using Diagnosis.Common;

namespace Diagnosis.Models
{
    public static class HrExtensions
    {
        public static Patient GetPatient(this HealthRecord hr)
        {
            return hr.Patient ?? (hr.Course != null ? hr.Course.Patient : hr.Appointment.Course.Patient);
        }

        public static Course GetCourse(this HealthRecord hr)
        {
            return hr.Course ?? (hr.Appointment != null ? hr.Appointment.Course : null);
        }
    }

    public static class IHrsHolderExtensions
    {
        public static IEnumerable<Word> GetAllWords(this Patient patient)
        {
            var pWords = patient.HealthRecords.SelectMany(hr => hr.Words);
            var cWords = patient.Courses.SelectMany(c => c.GetAllWords());
            return pWords.Union(cWords);
        }

        public static IEnumerable<Word> GetAllWords(this Course course)
        {
            var cWords = course.HealthRecords.SelectMany(hr => hr.Words);
            var appsWords = course.Appointments.SelectMany(app => app.GetAllWords());
            return cWords.Union(appsWords);
        }

        public static IEnumerable<Word> GetAllWords(this Appointment app)
        {
            return app.HealthRecords.SelectMany(hr => hr.Words);
        }

        /// <summary>
        /// Все слова из записей держателя и его вложенных держателей.
        /// </summary>
        /// <param name="holder"></param>
        /// <returns></returns>
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
            var copy = holder.HealthRecords.ToList();
            copy.Where(hr => hr.IsEmpty())
                .ForAll(hr => holder.RemoveHealthRecord(hr));
        }
    }

    public static class IDomainEntityExtensions
    {
        /// <summary>
        /// Определяет, пуста ли сущность.
        /// пациент  — без записей и курсов
        /// курс — без записей и осмотров
        /// осмотр — без записей
        /// запись — без элементов (слов, дат, комментариев). Категория не считается.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsEmpty(this IDomainObject entity)
        {
            var @switch = new Dictionary<Type, Func<bool>> {
                { typeof(Patient), () => 
                    {
                        var pat = entity as Patient;
                        return pat.Courses.All(x => x.IsEmpty() && pat.HealthRecords.All(h => h.IsEmpty()));
                    } 
                },
                { typeof(Course), () => 
                    {
                        var course = entity as Course;
                        return course.Appointments.All(x => x.IsEmpty() && course.HealthRecords.All(h => h.IsEmpty()));
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
                        return hr.Comment.IsNullOrEmpty()
                            && hr.DateOffset.IsEmpty
                            && hr.HrItems.Count() == 0;
                    }
                },
           };

            return @switch[entity.GetType()]();

        }
    }
}