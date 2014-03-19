using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

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
                if (CheckSnils(value))
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

        /// <summary>
        /// Добавляет свойство со значением или изменяет значение, если такое свойство уже есть.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public virtual void SetPropertyValue(Property property, PropertyValue value)
        {
            Contract.Requires(property != null);
            Contract.Requires(value != null);

            var existingPatientProperty = patientProperties.FirstOrDefault(
                pp => pp.Patient == this && pp.Property == property);

            if (existingPatientProperty == null)
            {
                patientProperties.Add(new PatientProperty(this, property, value));
            }
            else
            {
                existingPatientProperty.Value = value;
            }
        }

        static bool CheckSnils(string snils)
        {
            if (snils == null || snils.Length != 11)
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

        public Patient(string lastName, string firstName, DateTime birthDate, string middleName = null)
        {
            Contract.Requires(lastName != null);
            Contract.Requires(firstName != null);

            LastName = lastName;
            FirstName = firstName;
            MiddleName = middleName;
            BirthDate = birthDate;
            IsMale = true;
        }

        protected Patient()
        {
        }
    }
}
