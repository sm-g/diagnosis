using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
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
}