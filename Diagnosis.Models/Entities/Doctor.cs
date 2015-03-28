using Diagnosis.Common;
using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class Doctor : ValidatableEntity<Guid>, IDomainObject, IMan, IUser, IComparable<Doctor>
    {
        private Iesi.Collections.Generic.ISet<Appointment> appointments = new HashedSet<Appointment>();
        private Iesi.Collections.Generic.ISet<Course> courses = new HashedSet<Course>();
        private Iesi.Collections.Generic.ISet<Setting> settingsSet = new HashedSet<Setting>();
        private Iesi.Collections.Generic.ISet<HealthRecord> healthRecords = new HashedSet<HealthRecord>();

        private string _fn;
        private string _ln;
        private string _mn;
        private bool _isMale;
        private Speciality _speciality;
        private Passport passport;
        private SettingsProvider settingsProvider;
        private Vocabulary _customVocabulary;

        public Doctor(string lastName, string firstName = null, string middleName = null, Speciality speciality = null)
        {
            Contract.Requires(!lastName.IsNullOrEmpty());

            LastName = lastName;
            FirstName = firstName;
            MiddleName = middleName;
            Speciality = speciality;
            IsMale = true;
            Passport = new Passport(this);
        }

        protected Doctor()
        {
        }

        private List<Word> myVar = new List<Word>();
        /// <summary>
        /// Слова из всех словарей, доступных врачу, кроме пользовательского.
        /// </summary>
        public virtual List<Word> SpecialityWords
        {
            get { return myVar; }
            set { myVar = value; }
        }
        /// <summary>
        /// Все слова, доступные врачу.
        /// </summary>
        public virtual IEnumerable<Word> Words
        {
            get { return SpecialityWords.Union(CustomVocabulary.Words); }
        }

        public virtual string FirstName
        {
            get { return _fn; }
            set
            {
                SetProperty(ref _fn, value.TrimedOrNull(), "FirstName");
            }
        }

        public virtual string MiddleName
        {
            get { return _mn; }
            set
            {
                SetProperty(ref _mn, value.TrimedOrNull(), "MiddleName");
            }
        }

        public virtual string LastName
        {
            get { return _ln; }
            set
            {
                if (value == null) return;
                SetProperty(ref _ln, value.Trim(), "LastName");
            }
        }

        public virtual bool IsMale
        {
            get { return _isMale; }
            set { SetProperty(ref _isMale, value, () => IsMale); }
        }

        public virtual Iesi.Collections.Generic.ISet<Setting> SettingsSet
        {
            get { return settingsSet; }
        }

        public virtual Speciality Speciality
        {
            get { return _speciality; }
            set
            {
                if (value == Speciality.Null) value = null;
                SetProperty(ref _speciality, value, () => Speciality);
            }
        }
        public virtual Vocabulary CustomVocabulary
        {
            get { return _customVocabulary ?? (_customVocabulary = new Vocabulary(Vocabulary.CustomTitle, this)); }
            set
            {
                SetProperty(ref _customVocabulary, value, () => CustomVocabulary);
            }
        }

        public virtual Passport Passport
        {
            get { return passport; }
            set
            {
                SetProperty(ref passport, value, () => Passport);
            }
        }

        public virtual IEnumerable<Appointment> Appointments
        {
            get { return appointments; }
        }

        public virtual IEnumerable<Course> Courses
        {
            get { return courses; }
        }

        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get { return healthRecords.OrderBy(x => x.Ord); }
        }

        public virtual string FullName
        {
            get
            {
                return NameFormatter.GetFullName(this);
            }
        }

        public virtual SettingsProvider Settings
        {
            get { return settingsProvider ?? (settingsProvider = new SettingsProvider(this)); }
        }

        public virtual Course StartCourse(Patient patient)
        {
            Contract.Requires(patient != null);
            Contract.Ensures(patient.Courses.Contains(Contract.Result<Course>()));

            var course = new Course(patient, this);
            patient.AddCourse(course);
            return course;
        }

        public override string ToString()
        {
            return FullName;
        }

        public override ValidationResult SelfValidate()
        {
            return new DoctorValidator().Validate(this);
        }

        public virtual int CompareTo(Doctor other)
        {
            // по ФИО
            var byLast = this.LastName.CompareToNullSafe(other.LastName);
            if (byLast == 0)
            {
                var byFirst = this.FirstName.CompareToNullSafe(other.FirstName);
                if (byFirst == 0)
                {
                    var byMiddle = this.MiddleName.CompareToNullSafe(other.MiddleName);
                    return byMiddle;
                }
                return byFirst;
            }
            return byLast;
        }
    }
}