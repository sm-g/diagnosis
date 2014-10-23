using Diagnosis.Core;
using Diagnosis.Models;
using System;
using System.Collections.Generic;

namespace Diagnosis.ViewModels.Screens
{
    /// <summary>
    /// Хранит последние выбранные записи для каждого пациента, курса, осмотра.
    /// </summary>
    class HrViewer : NotifyPropertyChangedBase
    {
        private Dictionary<Patient, HealthRecord> patMap;
        private Dictionary<Course, HealthRecord> courseMap;
        private Dictionary<Appointment, HealthRecord> appMap;

        public HrViewer()
        {
            patMap = new Dictionary<Patient, HealthRecord>();
            courseMap = new Dictionary<Course, HealthRecord>();
            appMap = new Dictionary<Appointment, HealthRecord>();
        }

        internal void Select(HealthRecord hr, IHrsHolder holder)
        {
            var @switch = new Dictionary<Type, Action> {
                { typeof(Patient), () => OnHrOpened<Patient>(patMap, holder as Patient, hr)},
                { typeof(Course), () => OnHrOpened<Course>(courseMap, holder as Course, hr) },
                { typeof(Appointment), () => OnHrOpened<Appointment>(appMap, holder as Appointment, hr) }
            };
            @switch[holder.GetType()]();
        }

        public HealthRecord GetLastSelectedFor(IHrsHolder holder)
        {
            HealthRecord hr;
            if (holder is Patient && patMap.TryGetValue(holder as Patient, out hr))
                return hr;
            if (holder is Course && courseMap.TryGetValue(holder as Course, out hr))
                return hr;
            if (holder is Appointment && appMap.TryGetValue(holder as Appointment, out hr))
                return hr;

            return null;
        }

        private void OnHrOpened<T>(IDictionary<T, HealthRecord> dict, T holder, HealthRecord hr)
        {
            if (!dict.ContainsKey(holder))
            {
                dict.Add(holder, hr);
            }
            else
            {
                dict[holder] = hr;
            }
        }
    }
}