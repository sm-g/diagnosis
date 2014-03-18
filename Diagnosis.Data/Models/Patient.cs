using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace Diagnosis.Models
{
    public class Patient
    {
        ISet<PatientProperty> patientProperties = new HashSet<PatientProperty>();
        ISet<Course> courses = new HashSet<Course>();

        string _snils;

        public virtual int Id { get; protected set; }
        public virtual string FirstName { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual string LastName { get; set; }
        public virtual bool IsMale { get; set; }
        public virtual DateTime BirthDate { get; set; }
        public virtual string SNILS
        {
            get
            {
                return _snils;
            }
            set
            {
                if (_snils != value && CheckSnils(value))
                {
                    _snils = value;
                }
            }
        }

        public virtual ReadOnlyCollection<PatientProperty> PatientProperties
        {
            get
            {
                return new ReadOnlyCollection<PatientProperty>(
                    new List<PatientProperty>(patientProperties));
            }
        }

        public virtual ReadOnlyCollection<Course> Courses
        {
            get
            {
                return new ReadOnlyCollection<Course>(
                    new List<Course>(courses));
            }
        }

        public virtual void AddPatientProperty(PatientProperty property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            if (!patientProperties.Contains(property))
            {
                patientProperties.Add(property);
                property.Patient = this;
            }
        }

        static bool CheckSnils(string snils)
        {
            if (snils.Length != 11)
                return false;

            int number;
            int control;
            if (!int.TryParse(snils.Substring(0, 9), out number) ||
                !int.TryParse(snils.Substring(9, 2), out control))
                return false;

            if (number <= 1001998)
                return true;

            int sum = 0;
            for (int i = 1; i <= 9; i++)
            {
                sum += (number % 10) * i;
                number /= 10;
            }
            if (sum < 100)
            {
                return sum == control;
            }
            if (sum > 101)
            {
                return sum == control % 101;
            }
            return 0 == control;
        }

        public Patient()
        {
            BirthDate = new DateTime(1980, 6, 15);
            IsMale = true;
        }
    }
}
