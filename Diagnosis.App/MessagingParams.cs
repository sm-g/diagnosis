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

    class CategoryCheckedChangedParams : EventParams
    {
        public CategoryCheckedChangedParams(CategoryViewModel category, bool isChecked)
        {
            Contract.Requires(category != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Category, category),
                new KeyValuePair<string,object>(Messages.CheckedState, isChecked)
            };
        }
    }

    class WordCheckedChangedParams : EventParams
    {
        public WordCheckedChangedParams(WordViewModel symtpom, bool isChecked)
        {
            Contract.Requires(symtpom != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Word, symtpom),
                new KeyValuePair<string,object>(Messages.CheckedState, isChecked)
            };
        }
    }

    class DiagnosisCheckedChangedParams : EventParams
    {
        public DiagnosisCheckedChangedParams(DiagnosisViewModel diagnosis, bool isChecked)
        {
            Contract.Requires(diagnosis != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Diagnosis, diagnosis),
                new KeyValuePair<string,object>(Messages.CheckedState, isChecked)
            };
        }
    }

    class PatientCheckedChangedParams : EventParams
    {
        public PatientCheckedChangedParams(PatientViewModel patient, bool isChecked)
        {
            Contract.Requires(patient != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Patient, patient),
                new KeyValuePair<string,object>(Messages.CheckedState, isChecked)
            };
        }
    }

    class CurrentPatientChangedParams : EventParams
    {
        public CurrentPatientChangedParams(PatientViewModel patient)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.Patient, patient)
            };
        }
    }

    class CurrentDoctorChangedParams : EventParams
    {
        public CurrentDoctorChangedParams(DoctorViewModel doctor)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.Doctor, doctor)
            };
        }
    }

    class PropertySelectedValueChangedParams : EventParams
    {
        public PropertySelectedValueChangedParams(PropertyViewModel property)
        {
            Contract.Requires(property != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Property, property)
            };
        }
    }

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

    class AppointmentAddedParams : EventParams
    {
        public AppointmentAddedParams(AppointmentViewModel appointment)
        {
            Contract.Requires(appointment != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Appointment, appointment)
            };
        }
    }


    class DirectoryEditingModeChangedParams : EventParams
    {
        public DirectoryEditingModeChangedParams(bool isEditing)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.Boolean, isEditing)
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
    class OpenHealthRecordParams : HealthRecordParams
    {
        public OpenHealthRecordParams(HealthRecordViewModel hrVM) : base(hrVM) { }
    }
}
