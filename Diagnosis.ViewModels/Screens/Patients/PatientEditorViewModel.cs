using Diagnosis.Common;
using Diagnosis.Models;
using log4net;
using System.ComponentModel;

namespace Diagnosis.ViewModels.Screens
{
    public class PatientEditorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(PatientEditorViewModel));
        private readonly Patient patient;
        private PatientViewModel _vm;

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
                    Save();

                    (patient as IEditableObject).EndEdit();

                    using (var t = Session.BeginTransaction())
                    {
                        try
                        {
                            t.Commit();
                        }
                        catch (System.Exception e)
                        {
                            t.Rollback();
                            logger.Error(e);
                        }
                    }

                    DialogResult = true;
                }, () => CanSave());
            }
        }

        public RelayCommand SaveAndCreateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SaveCommand.Execute(null);

                    this.Send(Events.CreatePatient);
                }, () => CanSaveAndCreate());
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

                    this.Send(Events.OpenAppointment, app.AsParams(MessageKeys.Appointment));
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

        private void Save()
        {
            // получаем метку для нового
            bool updateLabel = false;
            if (patient.IsTransient)
                updateLabel = true;

            Session.SaveOrUpdate(patient);

            if (updateLabel)
                patient.Label = patient.Id.ToString();
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