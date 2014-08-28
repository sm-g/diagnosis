using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Diagnosis.Core;

namespace Diagnosis.Models
{
    public class Patient : IEntity
    {
        ISet<PatientRecordProperty> patientProperties = new HashSet<PatientRecordProperty>();
        ISet<Course> courses = new HashSet<Course>();

        int? _year;
        byte? _month;
        byte? _day;
        string _fn;
        string _ln;
        string _mn;
        string _label;
        private ObservableCollection<Course> _courses;

        public virtual int Id { get; protected set; }
        public virtual string Label
        {
            get
            {
                return _label;
            }
            set
            {
                _label = value.TrimedOrNull();
            }
        }
        public virtual string FirstName
        {
            get
            {
                return _fn;
            }
            set
            {
                _fn = value.TrimedOrNull();
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
                _mn = value.TrimedOrNull();
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
                _ln = value.TrimedOrNull();
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
                if (value == null)
                {
                    _year = value;
                }
                if (value <= DateTime.Today.Year)
                {
                    _year = value;
                }
                CheckDate();
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
                if (value == null)
                {
                    _month = value;
                }
                if (value >= 0 && value <= 12)
                {
                    _month = value > 0 ? value : null;
                }
                CheckDate();
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
                if (value == null)
                {
                    _day = value;
                }
                if (value >= 0 && value <= 31)
                {
                    _day = value > 0 ? value : null;
                }
                CheckDate();
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
        public virtual ObservableCollection<Course> Courses
        {
            get
            {

                return _courses ?? (_courses = new ObservableCollection<Course>(courses));
            }
        }

        public virtual int? Age
        {
            get
            {
                if (!BirthYear.HasValue)
                {
                    return null;
                }

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

                    DateHelper.CheckDate(year, BirthMonth, BirthDay);

                    BirthYear = year;
                }
            }
        }

        public virtual string FullName
        {
            get
            {
                return LastName + " " + FirstName + " " + MiddleName;
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
                patientProperties.Add(new PatientRecordProperty(property, value, this));
            }
            else
            {
                existingPatientProperty.Value = value;
            }
        }

        void CheckDate()
        {
            DateHelper.CheckDate(BirthYear, BirthMonth, BirthDay);
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

        public override string ToString()
        {
            return Id + " " + Label + " " + FullName;
        }
    }
}
