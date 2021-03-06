﻿using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class DoctorViewModel : ViewModelBase, IMan
    {
        internal readonly Doctor doctor;
        internal bool canEditComboBoxValues; // WPF ComboBox SelectedItem Set to Null when close editor

        public DoctorViewModel(Doctor d)
        {
            Contract.Requires(d != null);
            doctor = d;
            this.validatableEntity = d;
            doctor.PropertyChanged += doctor_PropertyChanged;
        }

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
                return doctor.Speciality ?? Speciality.Null;
            }
            set
            {
                if (canEditComboBoxValues)
                    doctor.Speciality = value;
            }
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