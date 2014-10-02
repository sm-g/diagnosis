using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Core;

namespace Diagnosis.Models
{
    public static class EmptyChecker
    {
        /// <summary>
        /// Определяет, пуста ли сущность.
        /// пациент  — без записей и курсов
        /// курс — без записей и осмотров
        /// осмотр — без записей
        /// запись — без элементов (слов, дат, комментариев). категория не считается.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsEmpty(this EntityBase entity)
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
                        return app.HealthRecords.All(x => x.IsEmpty() && app.HealthRecords.All(h => h.IsEmpty()));
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
