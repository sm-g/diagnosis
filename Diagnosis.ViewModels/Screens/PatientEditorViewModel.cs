using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventAggregator;
using Diagnosis.Core;


namespace Diagnosis.ViewModels
{
    public class PatientEditorViewModel : SessionVMBase
    {
        private readonly Patient patient;
        private PatientViewModel _vm;
        private bool shouldCommit;
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

        private bool CanSave()
        {
            return patient.IsTransient || patient.IsDirty;
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
                       }, () => patient.Courses.Count == 0);
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

            Session.BeginTransaction();
        }
        /// <summary>
        /// Создает нового пациента.
        /// </summary>
        public PatientEditorViewModel()
            : this(new Patient())
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (shouldCommit)
            {
                Session.Transaction.Commit();
            }
            else
            {
                if (Session.Transaction.IsActive)
                Session.Transaction.Rollback(); // если flush.never - не нужно?
            }

            base.Dispose(disposing);
        }
    }
}
