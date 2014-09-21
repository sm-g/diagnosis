using Diagnosis.Core;
using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class Patient : ValidatableEntity, IDomainEntity
    {
        private Iesi.Collections.Generic.ISet<PatientRecordProperty> _patientProperties = new HashedSet<PatientRecordProperty>();
        private Iesi.Collections.Generic.ISet<Course> courses = new HashedSet<Course>();

        private int? _year;
        private byte? _month;
        private byte? _day;
        private string _fn;
        private string _ln;
        private string _mn;
        private string _label;
        private bool _isMale;

        public virtual event NotifyCollectionChangedEventHandler CoursesChanged;


        public virtual string Label
        {
            get
            {
                return _label;
            }
            set
            {
                if (_label == value)
                    return;
                EditHelper.Edit("Label", _label);
                _label = value.TrimedOrNull();
                OnPropertyChanged("Label");
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
                if (_fn == value)
                    return;
                EditHelper.Edit("FirstName", _ln);
                _fn = value.TrimedOrNull();
                OnPropertyChanged("FirstName");
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
                if (_mn == value)
                    return;
                EditHelper.Edit("MiddleName", _mn);
                _mn = value.TrimedOrNull();
                OnPropertyChanged("MiddleName");
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
                if (_ln == value)
                    return;
                EditHelper.Edit("LastName", _ln);
                _ln = value.TrimedOrNull();
                OnPropertyChanged("LastName");
            }
        }

        public virtual bool IsMale
        {
            get { return _isMale; }
            set
            {
                if (_isMale == value)
                    return;
                EditHelper.Edit("IsMale", _isMale);
                _isMale = value;
                OnPropertyChanged("IsMale");
            }
        }

        public virtual int? BirthYear
        {
            get
            {
                return _year;
            }
            set
            {
                if (_year == value)
                    return;

                if (value == null || value <= DateTime.Today.Year)
                {
                    EditHelper.Edit("BirthYear", _year);
                    _year = value;
                }
                CheckDate();
                OnPropertyChanged("BirthYear");
                OnPropertyChanged("Age");
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
                if (_month == value)
                    return;

                EditHelper.Edit("BirthMonth", _month);

                _month = value >= 0 && value <= 12 ? value : null;

                CheckDate();
                OnPropertyChanged("BirthMonth");
                OnPropertyChanged("Age");
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
                if (_day == value)
                    return;

                EditHelper.Edit("BirthDay", _day);

                _day = value >= 0 && value <= 31 ? value : null;

                CheckDate();
                OnPropertyChanged("BirthDay");
                OnPropertyChanged("Age");
            }
        }

        public virtual IEnumerable<PatientRecordProperty> PatientProperties
        {
            get
            {
                return _patientProperties;
            }
        }

        public virtual IEnumerable<Course> Courses
        {
            get
            {
                return courses;
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
                else
                {
                    BirthYear = null;
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

            var existingPatientProperty = _patientProperties.FirstOrDefault(
                pp => pp.Patient == this && pp.Property == property);

            EditHelper.Edit("PatientProperties", _patientProperties);

            if (existingPatientProperty == null)
            {
                _patientProperties.Add(new PatientRecordProperty(property, value, this));
            }
            else
            {
                existingPatientProperty.Value = value;
            }
            OnPropertyChanged("PatientProperties");
        }

        private void CheckDate()
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
            Label = new Random().Next(10000, 99999).ToString();
        }

        protected Patient()
        {

        }

        public override string ToString()
        {
            return Id + " " + Label + " " + FullName;
        }

        protected virtual void OnCoursesChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = CoursesChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        protected internal virtual void AddCourse(Course course)
        {
            if (!courses.Contains(course))
            {
                courses.Add(course);
                OnCoursesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, course));
            }
        }

        public override ValidationResult SelfValidate()
        {
            return new PatientValidator().Validate(this);
        }
    }
}