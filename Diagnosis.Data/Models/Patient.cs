using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class Patient
    {
        ISet<PatientRecordProperty> patientProperties = new HashSet<PatientRecordProperty>();
        ISet<Course> courses = new HashSet<Course>();

        string _snils;
        int? _year;
        byte? _month;
        byte? _day;
        string _fn;
        string _ln;
        string _mn;

        public virtual int Id { get; protected set; }
        public virtual string FirstName
        {
            get
            {
                return _fn;
            }
            set
            {
                if (value == "")
                {
                    _fn = null;
                }
                else
                {
                    _fn = value;
                }
            }
        }
        public virtual string MiddleName
        {
            get
            {
                return _mn;
            }
            set
            {
                if (value == "")
                {
                    _mn = null;
                }
                else
                {
                    _mn = value;
                }
            }
        }
        public virtual string LastName
        {
            get
            {
                return _ln;
            }
            set
            {
                if (value == "")
                {
                    _ln = null;
                }
                else
                {
                    _ln = value;
                }
            }
        }
        public virtual bool IsMale { get; set; }
        public virtual int? BirthYear
        {
            get
            {
                return _year;
            }
            set
            {
                if (value <= DateTime.Today.Year)
                {
                    _year = value;
                    CheckDate();
                }
            }
        }
        public virtual byte? BirthMonth
        {
            get
            {
                return _month;
            }
            set
            {
                if (value >= 0 && value <= 12)
                {
                    _month = value > 0 ? value : null;
                    CheckDate();

                }
            }
        }
        public virtual byte? BirthDay
        {
            get
            {
                return _day;
            }
            set
            {
                if (value >= 0 && value <= 31)
                {
                    _day = value > 0 ? value : null;
                    CheckDate();
                }
            }
        }

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
        public virtual ReadOnlyCollection<PatientRecordProperty> PatientProperties
        {
            get
            {
                return new ReadOnlyCollection<PatientRecordProperty>(
                    new List<PatientRecordProperty>(patientProperties));
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

        public virtual int? Age
        {
            get
            {
                int age = DateTime.Today.Year - BirthYear.Value;

                try
                {
                    if (new DateTime(BirthYear.Value, BirthMonth.Value, BirthDay.Value) > DateTime.Today.AddYears(-age))
                        age--;
                }
                catch
                {
                }
                return age;
            }
            set
            {
                if (value.HasValue)
                {
                    int year = DateTime.Today.Year - value.Value;

                    if (BirthMonth.HasValue && BirthDay.HasValue &&
                        new DateTime(year, BirthMonth.Value, BirthDay.Value) > DateTime.Today.AddYears(-value.Value))
                        year--;

                    Checkers.CheckDate(year, BirthMonth, BirthDay);

                    BirthYear = year;
                }
            }
        }

        public virtual void DeleteCourse(Course course)
        {
            courses.Remove(course);
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
                patientProperties.Add(new PatientRecordProperty(property, value, this));
            }
            else
            {
                existingPatientProperty.Value = value;
            }
        }

        void CheckDate()
        {
            Checkers.CheckDate(BirthYear, BirthMonth, BirthDay);
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

        public Patient(string lastName = null,
            string firstName = null,
            string middleName = null,
            int? year = null,
            byte? month = null,
            byte? day = null,
            bool isMale = true)
        {
            LastName = lastName;
            FirstName = firstName;
            MiddleName = middleName;
            BirthYear = year;
            BirthMonth = month;
            BirthDay = day;
            IsMale = isMale;
        }

        protected Patient()
        {
        }
    }
}
