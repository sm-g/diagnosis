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
    public class Patient : ValidatableEntity, IDomainObject, IHrsHolder, IMan
    {
        private Iesi.Collections.Generic.ISet<Course> courses = new HashedSet<Course>();
        Iesi.Collections.Generic.ISet<HealthRecord> healthRecords = new HashedSet<HealthRecord>();

        public virtual event NotifyCollectionChangedEventHandler HealthRecordsChanged;

        private int? _year;
        private byte? _month;
        private byte? _day;
        private string _fn;
        private string _ln;
        private string _mn;
        private string _label;
        private bool? _isMale;

        public virtual event NotifyCollectionChangedEventHandler CoursesChanged;


        public virtual string Label
        {
            get { return _label ?? Id.ToString(); }
            set { SetProperty(ref _label, value.TrimedOrNull(), () => Label); }
        }

        public virtual string FirstName
        {
            get { return _fn; }
            set { SetProperty(ref _fn, value.TrimedOrNull(), () => FirstName); }
        }

        public virtual string MiddleName
        {
            get { return _mn; }
            set { SetProperty(ref _mn, value.TrimedOrNull(), () => MiddleName); }

        }

        public virtual string LastName
        {
            get { return _ln; }
            set { SetProperty(ref _ln, value.TrimedOrNull(), () => LastName); }
        }

        public virtual bool? IsMale
        {
            get { return _isMale; }
            set { SetProperty(ref _isMale, value, () => IsMale); }
        }

        public virtual int? BirthYear
        {
            get { return _year; }
            set
            {
                if (SetProperty(ref _year, value, () => BirthYear))
                    OnPropertyChanged("Age");
            }
        }

        public virtual byte? BirthMonth
        {
            get { return _month; }
            set
            {
                if (SetProperty(ref _month, value, () => BirthMonth))
                    OnPropertyChanged("Age");
            }
        }

        public virtual byte? BirthDay
        {
            get { return _day; }
            set
            {
                if (SetProperty(ref _day, value, () => BirthDay))
                    OnPropertyChanged("Age");
            }
        }
        public virtual IEnumerable<Course> Courses
        {
            get { return courses; }
        }

        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get { return healthRecords; }
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
                    // корректируем возраст только если указана полная дата рождения
                }
                return age;
            }
            set
            {
                if (value.HasValue)
                {
                    // установка возраста меняет только год рождения
                    int year = DateTime.Today.Year - value.Value;
                    if (BirthMonth.HasValue && BirthDay.HasValue)
                    {
                        DateHelper.CheckAndCorrectDate((int?)year, ref _month, ref _day);
                        if (new DateTime(year, BirthMonth.Value, BirthDay.Value) > DateTime.Today.AddYears(-value.Value))
                            year--;
                    }
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

        public Patient(string lastName = null,
            string firstName = null,
            string middleName = null,
            int? year = null,
            byte? month = null,
            byte? day = null,
            bool? isMale = null)
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
        public virtual HealthRecord AddHealthRecord()
        {
            var hr = new HealthRecord(this);
            healthRecords.Add(hr);
            OnHealthRecordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, hr));

            return hr;
        }

        public virtual void RemoveHealthRecord(HealthRecord hr)
        {
            if (healthRecords.Remove(hr))
                OnHealthRecordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, hr));
        }

        protected internal virtual void AddCourse(Course course)
        {
            if (!courses.Contains(course))
            {
                courses.Add(course);
                OnCoursesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, course));
            }
        }
        public virtual void RemoveCourse(Course course)
        {
            Contract.Requires(course.IsEmpty());

            if (courses.Remove(course))
            {
                OnCoursesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, course));
            }
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
        protected virtual void OnHealthRecordsChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = HealthRecordsChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        public override ValidationResult SelfValidate()
        {
            return new PatientValidator().Validate(this);
        }
    }
}