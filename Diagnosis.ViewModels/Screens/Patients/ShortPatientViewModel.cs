﻿using Diagnosis.Models;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels.Screens
{
    /// <summary>
    /// Readonly viewmodel of Patient without associated courses.
    /// </summary>
    public class ShortPatientViewModel : CheckableBase
    {
        internal readonly Patient patient;

        public ShortPatientViewModel(Patient p)
        {
            Contract.Requires(p != null);
            this.patient = p;
        }

        public string FirstName
        {
            get
            {
                return patient.FirstName ?? "";
            }
        }

        public string MiddleName
        {
            get
            {
                return patient.MiddleName ?? "";
            }
        }

        public string LastName
        {
            get
            {
                return patient.LastName ?? "";
            }
        }

        public int? Age
        {
            get
            {
                return patient.Age;
            }
        }

        public int? BirthYear
        {
            get
            {
                return patient.BirthYear;
            }
        }

        public int? BirthMonth
        {
            get
            {
                return patient.BirthMonth;
            }
        }

        public int? BirthDay
        {
            get
            {
                return patient.BirthDay;
            }
        }

        public bool? IsMale
        {
            get
            {
                return patient.IsMale;
            }
        }

        public bool NoName
        {
            get
            {
                return patient.LastName == null && patient.MiddleName == null && patient.FirstName == null;
            }
        }
        public override string ToString()
        {
            return string.Format("{0} of {1}", this.GetType().Name, patient.ToString());
        }
    }
}