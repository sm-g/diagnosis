using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class DoctorViewModel : ViewModelBase, IMan
    {
        internal readonly Doctor doctor;

        public string FirstName
        {
            get
            {
                return doctor.FirstName;
            }
            set
            {
                doctor.FirstName = value;
            }
        }

        public string MiddleName
        {
            get
            {
                return doctor.MiddleName;
            }
            set
            {
                doctor.MiddleName = value;
            }
        }

        public string LastName
        {
            get
            {
                return doctor.LastName;
            }
            set
            {
                doctor.LastName = value;
            }
        }

        public bool IsMale
        {
            get
            {
                return doctor.IsMale;
            }
            set
            {
                if (value)
                    doctor.IsMale = true;
            }
        }

        public bool IsFemale
        {
            get
            {
                return !doctor.IsMale;
            }
            set
            {
                if (value)
                    doctor.IsMale = false;
            }
        }

        public Speciality Speciality
        {
            get
            {
                return doctor.Speciality;
            }
            set
            {
                doctor.Speciality = value;
            }
        }

        public override string this[string columnName]
        {
            get
            {
                var results = doctor.SelfValidate();
                if (results == null)
                    return string.Empty;
                var message = results.Errors
                    .Where(x => x.PropertyName == columnName)
                    .Select(x => x.ErrorMessage)
                    .FirstOrDefault();
                return message != null ? message : string.Empty;
            }
        }

        public DoctorViewModel(Doctor d)
        {
            Contract.Requires(d != null);
            doctor = d;
            doctor.PropertyChanged += doctor_PropertyChanged;
        }

        private void doctor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public override string ToString()
        {
            return doctor.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                doctor.PropertyChanged -= doctor_PropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}