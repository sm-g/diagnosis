using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using log4net;
using System.Linq;
using System.ComponentModel;

namespace Diagnosis.ViewModels.Screens
{
    public class PatientEditorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(PatientEditorViewModel));
        private readonly Patient patient;
        private PatientViewModel _vm;

        /// <summary>
        /// Начинает редактировать пациента.
        /// </summary>
        /// <param name="patient"></param>
        public PatientEditorViewModel(Patient patient)
        {
            this.patient = patient;

            Patient = new PatientViewModel(patient);
            (patient as IEditableObject).BeginEdit();

            Title = "Данные пациента";
        }

        /// <summary>
        /// Начинает редактировать нового пациента.
        /// </summary>
        public PatientEditorViewModel()
            : this(new Patient())
        { }

        public PatientViewModel Patient
        {
            get
            {
                return _vm;
            }
            private set
            {
                if (_vm != value)
                {
                    _vm = value;
                    OnPropertyChanged(() => Patient);
                }
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    (patient as IEditableObject).EndEdit();
                    if (patient.BirthYear == null)
                    {
                        // нельзя рассчитать возраст
                        // меняем Unit всех записей c ByAge на NotSet
                        foreach (var hr in patient.GetAllHrs().Where(hr => hr.Unit == HealthRecordUnit.ByAge))
                        {
                            hr.Unit = HealthRecordUnit.NotSet;
                        }
                    }
                    new Saver(Session).Save(patient);

                    this.Send(Event.PatientSaved, patient.AsParams(MessageKeys.Patient));
                    DialogResult = true;
                }, () => CanSave());
            }
        }

        public RelayCommand SaveAndOpenAppCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SaveCommand.Execute(null);

                    var course = AuthorityController.CurrentDoctor.StartCourse(patient);
                    var app = course.AddAppointment(AuthorityController.CurrentDoctor);

                    this.Send(Event.OpenAppointment, app.AsParams(MessageKeys.Appointment));
                }, () => CanSaveAndOpenApp());
            }
        }

        public bool IsUnsaved
        {
            get
            {
                return patient.IsTransient;
            }
        }
        private bool CanSave()
        {
            return (patient.IsTransient || patient.IsDirty) && patient.IsValid(); // есть изменения или при создании пациента
        }

        private bool CanSaveAndCreate()
        {
            return patient.IsValid();
        }

        private bool CanSaveAndOpenApp()
        {
            return patient.IsTransient && patient.IsValid(); // только при создании пациента
        }

        protected override void OnCancel()
        {
            (patient as IEditableObject).CancelEdit();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Patient.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}