using Diagnosis.Models;
using Diagnosis.App.ViewModels;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows;

namespace Diagnosis.App
{
    abstract class EventParams
    {
        public KeyValuePair<string, object>[] Params { get; protected set; }
    }

    #region with Models
    class CourseStartedParams : EventParams
    {
        public CourseStartedParams(Course course)
        {
            Contract.Requires(course != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Course, course)
            };
        }
    }


    class OpenHealthRecordParams : EventParams
    {
        public OpenHealthRecordParams(HealthRecord hr)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.HealthRecord, hr)
            };
        }
    }

    #endregion

    #region with ViewModels

    class CategoryCheckedChangedParams : EventParams
    {
        public CategoryCheckedChangedParams(CategoryViewModel categoryVM, bool isChecked)
        {
            Contract.Requires(categoryVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Category, categoryVM),
                new KeyValuePair<string,object>(Messages.CheckedState, isChecked)
            };
        }
    }

    class WordCheckedChangedParams : EventParams
    {
        public WordCheckedChangedParams(WordViewModel wordVM, bool isChecked)
        {
            Contract.Requires(wordVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Word, wordVM),
                new KeyValuePair<string,object>(Messages.CheckedState, isChecked)
            };
        }
    }

    class DiagnosisCheckedChangedParams : EventParams
    {
        public DiagnosisCheckedChangedParams(DiagnosisViewModel diagnosisVM, bool isChecked)
        {
            Contract.Requires(diagnosisVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Diagnosis, diagnosisVM),
                new KeyValuePair<string,object>(Messages.CheckedState, isChecked)
            };
        }
    }

    class PatientCheckedChangedParams : EventParams
    {
        public PatientCheckedChangedParams(PatientViewModel patientVM, bool isChecked)
        {
            Contract.Requires(patientVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Patient, patientVM),
                new KeyValuePair<string,object>(Messages.CheckedState, isChecked)
            };
        }
    }

    class CurrentPatientChangedParams : EventParams
    {
        public CurrentPatientChangedParams(PatientViewModel patientVM)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.Patient, patientVM)
            };
        }
    }

    class CurrentDoctorChangedParams : EventParams
    {
        public CurrentDoctorChangedParams(DoctorViewModel doctorVM)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.Doctor, doctorVM)
            };
        }
    }

    class PropertySelectedValueChangedParams : EventParams
    {
        public PropertySelectedValueChangedParams(PropertyViewModel propertyVM)
        {
            Contract.Requires(propertyVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Property, propertyVM)
            };
        }
    }


    class AppointmentAddedParams : EventParams
    {
        public AppointmentAddedParams(AppointmentViewModel appointmentVM)
        {
            Contract.Requires(appointmentVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Appointment, appointmentVM)
            };
        }
    }

    abstract class HealthRecordParams : EventParams
    {
        public HealthRecordParams(HealthRecordViewModel hrVM)
        {
            Contract.Requires(hrVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.HealthRecord, hrVM)
            };
        }
    }

    class HealthRecordChangedParams : HealthRecordParams
    {
        public HealthRecordChangedParams(HealthRecordViewModel hrVM) : base(hrVM) { }
    }
    class HealthRecordSelectedParams : HealthRecordParams
    {
        public HealthRecordSelectedParams(HealthRecordViewModel hrVM) : base(hrVM) { }
    }

    #endregion

    class DirectoryEditingModeChangedParams : EventParams
    {
        public DirectoryEditingModeChangedParams(bool isEditing)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.Boolean, isEditing)
            };
        }
    }
}
