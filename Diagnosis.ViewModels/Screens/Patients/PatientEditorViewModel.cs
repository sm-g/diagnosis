using Diagnosis.Core;
using Diagnosis.Models;
using log4net;
using System.ComponentModel;

namespace Diagnosis.ViewModels
{
    public class PatientEditorViewModel : ScreenBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(PatientEditorViewModel));
        private readonly Patient patient;
        private PatientViewModel _vm;
        private bool shouldCommit;
        private bool updateLabel = false;

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
                    OnPropertyChanged(() => IsUnsaved);
                }
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (patient.IsTransient)
                        updateLabel = true;
                    Session.SaveOrUpdate(patient);
                    if (updateLabel)
                        patient.Label = patient.Id.ToString();
                    shouldCommit = true;

                    this.Send(Events.LeavePatientEditor, patient.AsParams(MessageKeys.Patient));
                }, () => CanSave());
            }
        }

        public RelayCommand SaveAndCreateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (patient.IsTransient)
                        updateLabel = true;
                    Session.SaveOrUpdate(patient);
                    if (updateLabel)
                        patient.Label = patient.Id.ToString();
                    shouldCommit = true;

                    this.Send(Events.AddPatient);
                }, () => CanSave());
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    shouldCommit = false;

                    this.Send(Events.LeavePatientEditor, patient.AsParams(MessageKeys.Patient));
                });
            }
        }

        public RelayCommand StartCourseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Session.SaveOrUpdate(patient); // no
                    shouldCommit = true;

                    var course = AuthorityController.CurrentDoctor.StartCourse(patient);
                    this.Send(Events.OpenCourse, course.AsParams(MessageKeys.Course));
                });
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

        private bool CanSave()
        {
            return (patient.IsTransient || patient.IsDirty) && patient.IsValid();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (shouldCommit && patient.IsValid())
                {
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
                }
                else
                {
                    (patient as IEditableObject).CancelEdit();
                }
                Patient.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}