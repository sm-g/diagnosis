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
        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                                          {
                                              this.Send(Events.PatientSaved, patient.AsParams(MessageKeys.Patient));
                                          });
            }
        }

        public RelayCommand SaveAndCreateCommand
        {
            get
            {
                return new RelayCommand(() =>
                       {

                       });
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return new RelayCommand(() =>
                       {

                       });
            }
        }

        public RelayCommand FirstHrCommand
        {
            get
            {
                return new RelayCommand(() =>
                       {
                           this.Send(Events.FirstHr, patient.AsParams(MessageKeys.Patient));
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
        }
        /// <summary>
        /// Создает нового пациента.
        /// </summary>
        public PatientEditorViewModel()
            : this(new Patient())
        {
        }
    }
}
