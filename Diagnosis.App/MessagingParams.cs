using Diagnosis.Models;
using Diagnosis.App.ViewModels;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows;

namespace Diagnosis.App.Messaging
{
    abstract class EventParams
    {
        public KeyValuePair<string, object>[] Params { get; protected set; }
    }

    #region with Models
    class CourseModelParams : EventParams
    {
        public CourseModelParams(Course course)
        {
            Contract.Requires(course != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Course, course)
            };
        }
    }


    class HealthRecordModelParams : EventParams
    {
        public HealthRecordModelParams(HealthRecord hr)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.HealthRecord, hr)
            };
        }
    }

    class DoctorModelParams : EventParams
    {
        public DoctorModelParams(Doctor doctor)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.Doctor, doctor)
            };
        }
    }
    #endregion

    #region with ViewModels

    class CategoryParams : EventParams
    {
        public CategoryParams(CategoryViewModel categoryVM)
        {
            Contract.Requires(categoryVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Category, categoryVM)
            };
        }
    }

    class WordParams : EventParams
    {
        public WordParams(WordViewModel wordVM)
        {
            Contract.Requires(wordVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Word, wordVM)
            };
        }
    }
    class WordsParams : EventParams
    {
        public WordsParams(IEnumerable<WordViewModel> wordVMs)
        {
            Contract.Requires(wordVMs != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Word, wordVMs)
            };
        }
    }

    class DiagnosisParams : EventParams
    {
        public DiagnosisParams(DiagnosisViewModel diagnosisVM)
        {
            Contract.Requires(diagnosisVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Diagnosis, diagnosisVM)
            };
        }
    }

    class PatientParams : EventParams
    {
        public PatientParams(PatientViewModel patientVM)
        {
            Params = new[] {
                new KeyValuePair<string,object>(Messages.Patient, patientVM)
            };
        }
    }

    class PropertyParams : EventParams
    {
        public PropertyParams(PropertyViewModel propertyVM)
        {
            Contract.Requires(propertyVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Property, propertyVM)
            };
        }
    }


    class AppointmentParams : EventParams
    {
        public AppointmentParams(AppointmentViewModel appointmentVM)
        {
            Contract.Requires(appointmentVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Appointment, appointmentVM)
            };
        }
    }

    class HealthRecordParams : EventParams
    {
        public HealthRecordParams(HealthRecordViewModel hrVM)
        {
            Contract.Requires(hrVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.HealthRecord, hrVM)
            };
        }
    }
    class HealthRecordsParams : EventParams
    {
        public HealthRecordsParams(IEnumerable<HealthRecordViewModel> hrVMs)
        {
            Contract.Requires(hrVMs != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.HealthRecord, hrVMs)
            };
        }
    }

    class SettingsParams : EventParams
    {
        public SettingsParams(SettingsViewModel settingsVM)
        {
            Contract.Requires(settingsVM != null);

            Params = new[] {
                new KeyValuePair<string,object>(Messages.Settings, settingsVM)
            };
        }
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
