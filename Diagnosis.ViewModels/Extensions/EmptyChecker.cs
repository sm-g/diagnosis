using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Core;

namespace Diagnosis.ViewModels
{
    public static class EmptyChecker
    {
        /// <summary>
        /// Определяет, пуста ли сущность. 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsEmpty(this EntityBase entity)
        {
            var @switch = new Dictionary<Type, Func<bool>> {
                { typeof(Patient), () => 
                    {
                        var pat = entity as Patient;
                        return pat.Courses.All(x => x.IsEmpty());
                    } 
                },
                { typeof(Course), () => 
                    {
                        var course = entity as Course;
                        return course.Appointments.All(x => x.IsEmpty());
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
                            && hr.Measures.Count() ==0
                            && hr.DateOffset.IsEmpty
                            && hr.Disease== null
                            && hr.Symptom == null;
                    }
                },
           };

            return @switch[entity.GetType()]();

        }
    }
}
