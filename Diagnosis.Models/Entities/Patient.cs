using Diagnosis.Common;
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
    public class Patient : ValidatableEntity<Guid>, IDomainObject, IHaveAuditInformation, IHrsHolder, IMan, IComparable<Patient>
    {
        private Iesi.Collections.Generic.ISet<Course> courses = new HashedSet<Course>();
        private Iesi.Collections.Generic.ISet<HealthRecord> healthRecords = new HashedSet<HealthRecord>();

        private int? _year;
        private int? _month;
        private int? _day;
        private string _fn;
        private string _ln;
        private string _mn;
        private bool? _isMale;
        private DateTime _updatedAt;
        private DateTime _createdAt;

        public Patient(string lastName = null,
            string firstName = null,
            string middleName = null,
            int? year = null,
            int? month = null,
            int? day = null,
            bool? isMale = null)
            : this()
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
            _createdAt = DateTime.Now;
            _updatedAt = DateTime.Now;
        }

        public virtual event NotifyCollectionChangedEventHandler HealthRecordsChanged;

        public virtual event NotifyCollectionChangedEventHandler CoursesChanged;

        public virtual string FirstName
        {
            get { return _fn; }
            set
            {
                if (SetProperty(ref _fn, value.TrimedOrNull(), () => FirstName))
                {
                    OnPropertyChanged(() => FullName);
                    OnPropertyChanged(() => FullNameOrCreatedAt);
                }
            }
        }

        public virtual string MiddleName
        {
            get { return _mn; }
            set
            {
                if (SetProperty(ref _mn, value.TrimedOrNull(), () => MiddleName))
                {
                    OnPropertyChanged(() => FullName);
                    OnPropertyChanged(() => FullNameOrCreatedAt);
                }
            }
        }

        public virtual string LastName
        {
            get { return _ln; }
            set
            {
                if (SetProperty(ref _ln, value.TrimedOrNull(), () => LastName))
                {
                    OnPropertyChanged(() => FullName);
                    OnPropertyChanged(() => FullNameOrCreatedAt);
                }
            }
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
                {
                    OnPropertyChanged("Age");
                }
            }
        }

        public virtual int? BirthMonth
        {
            get { return _month; }
            set
            {
                if (SetProperty(ref _month, value, () => BirthMonth))
                    OnPropertyChanged("Age");
            }
        }

        public virtual int? BirthDay
        {
            get { return _day; }
            set
            {
                if (SetProperty(ref _day, value, () => BirthDay))
                    OnPropertyChanged("Age");
            }
        }

        public virtual DateTime CreatedAt
        {
            get { return _createdAt; }
        }

        DateTime IHaveAuditInformation.CreatedAt
        {
            get { return _updatedAt; }
            set
            {
                _createdAt = value;
            }
        }

        DateTime IHaveAuditInformation.UpdatedAt
        {
            get { return _updatedAt; }
            set { SetProperty(ref _updatedAt, value, () => UpdatedAt); }
        }

        public virtual DateTime UpdatedAt
        {
            get { return _updatedAt; }
        }

        public virtual IEnumerable<Course> Courses
        {
            get { return courses; }
        }

        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get { return healthRecords.OrderBy(x => x.Ord); }
        }

        public virtual int? Age
        {
            get
            {
                return DateHelper.GetAge(BirthYear, BirthMonth, BirthDay, DateTime.Now);
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

        /// <summary>
        /// Полное имя.
        /// </summary>
        public virtual string FullName
        {
            get
            {
                return NameFormatter.GetFullName(this);
            }
        }

        /// <summary>
        /// Полное имя или время создания.
        /// </summary>
        public virtual string FullNameOrCreatedAt
        {
            get
            {
                return NameFormatter.GetFullName(this) ?? CreatedAt.ToString("dd.MM.yy hh:mm");
            }
        }

        /// <summary>
        /// Дата последнего обновления записей внутри пациента.
        /// Дата обновления пациента, если запсией нет.
        /// </summary>
        public virtual DateTime LastHrUpdatedAt
        {
            get
            {
                if (this.GetAllHrs().Any())
                    return this.GetAllHrs()
                        .OrderByDescending(x => x.UpdatedAt)
                        .First().UpdatedAt;
                else
                    return this.UpdatedAt;
            }
        }

        public virtual HealthRecord AddHealthRecord(Doctor author)
        {
            var hr = new HealthRecord(this, author);
            healthRecords.Add(hr);
            author.AddHr(hr);
            OnHealthRecordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, hr));

            return hr;
        }

        public virtual void RemoveHealthRecord(HealthRecord hr)
        {
            if (healthRecords.Remove(hr))
            {
                hr.OnDelete();
                OnHealthRecordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, hr));
            }
        }

        public virtual Course AddCourse(Doctor doctor)
        {
            Contract.Requires(doctor != null);
            Contract.Ensures(Contract.Result<Course>().Patient.Equals(this));

            var course = new Course(this, doctor);

            courses.Add(course);
            doctor.AddCourse(course);

            OnCoursesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, course));
            return course;
        }

        public virtual void RemoveCourse(Course course)
        {
            Contract.Requires(course.IsEmpty());

            if (courses.Remove(course))
            {
                course.LeadDoctor.RemoveCourse(course);
                OnCoursesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, course));
            }
        }

        /// <summary>
        /// Курсы, отсорированные по дате. Первый — самый ранний курс.
        /// </summary>
        public virtual IEnumerable<Course> GetOrderedCourses()
        {
            return Courses.OrderBy(c => c);
        }

        public override string ToString()
        {
            return this.ShortId() + " " + FullName;
        }

        public override ValidationResult SelfValidate()
        {
            return new PatientValidator().Validate(this);
        }

        public virtual int CompareTo(IHrsHolder h)
        {
            var pat = h as Patient;
            if (pat != null)
                return this.CompareTo(pat);

            return 1;
        }

        public virtual int CompareTo(Patient other)
        {
            // по ФИО и дате обновления
            var byLast = this.LastName.CompareToNullSafe(other.LastName);
            if (byLast == 0)
            {
                var byFirst = this.FirstName.CompareToNullSafe(other.FirstName);
                if (byFirst == 0)
                {
                    var byMiddle = this.MiddleName.CompareToNullSafe(other.MiddleName);
                    if (byMiddle == 0)
                        return this.UpdatedAt.CompareTo(other.UpdatedAt);

                    return byMiddle;
                }
                return byFirst;
            }
            return byLast;
        }

        protected internal virtual void AddCourse(Course course)
        {
            Contract.Requires(course.Patient == null);
            //Contract.Requires(course.LeadDoctor != null);

            if (!courses.Contains(course))
            {
                course.Patient = this;
                courses.Add(course);
                OnCoursesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, course));
            }
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
    }
}