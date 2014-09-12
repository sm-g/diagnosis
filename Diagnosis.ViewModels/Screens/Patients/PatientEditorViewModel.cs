using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;
using System.ComponentModel;
using System.Linq;
using Diagnosis.Data;

namespace Diagnosis.ViewModels
{
    public class PatientEditorViewModel : SessionVMBase
    {
        private readonly Patient patient;
        private PatientViewModel _vm;
        private bool shouldCommit;
        private bool canFirstHr;

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
                    Session.SaveOrUpdate(patient);
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
                    Session.SaveOrUpdate(patient);
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

        public RelayCommand FirstHrCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Session.SaveOrUpdate(patient);
                    shouldCommit = true;

                    this.Send(Events.FirstHr, patient.AsParams(MessageKeys.Patient));
                }, () => canFirstHr);
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
            patient.CoursesChanged += patient_CoursesChanged;

            canFirstHr = patient.Courses.Count() == 0;

            Patient = new PatientViewModel(patient);

            Session.BeginTransaction();
            (patient as IEditableObject).BeginEdit();
        }
        /// <summary>
        /// Начинает редактировать нового пациента.
        /// </summary>
        public PatientEditorViewModel()
            : this(new Patient())
        {
        }

        private void patient_CoursesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            canFirstHr = patient.Courses.Count() == 0;
        }

        private bool CanSave()
        {
            return patient.IsTransient || patient.IsDirty;
        }

        protected override void Dispose(bool disposing)
        {
            if (shouldCommit)
            {
                (patient as IEditableObject).EndEdit();
                Session.Transaction.Commit();
            }
            else
            {
                (patient as IEditableObject).CancelEdit();
            }
            Session.Transaction.Dispose();
            Patient.Dispose();
            patient.CoursesChanged -= patient_CoursesChanged;

            base.Dispose(disposing);
        }
    }
}