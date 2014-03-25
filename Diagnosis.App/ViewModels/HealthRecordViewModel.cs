using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class HealthRecordViewModel : CheckableBase
    {
        private readonly HealthRecord healthRecord;
        private static HealthRecordViewModel current;
        private DiagnosisViewModel _diagnosis;
        private bool _selectingSymptomsActive;
        private List<EventMessageHandler> msgHandlers;

        #region CheckableBase

        public override string Name
        {
            get
            {
                return (Diagnosis != null ? Diagnosis.Name + ". " : "") +
                    string.Concat(Symptoms.OrderBy(s => s.Priority).Select(s => s.Name + " "));
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected override void OnCheckedChanged()
        {
            throw new NotImplementedException();
        }

        #endregion CheckableBase

        public string Description
        {
            get
            {
                return healthRecord.Description;
            }
            set
            {
                if (healthRecord.Description != value)
                {
                    healthRecord.Description = value;
                    OnPropertyChanged(() => Description);
                }
            }
        }

        public ObservableCollection<SymptomViewModel> Symptoms
        {
            get;
            private set;
        }

        public DiagnosisViewModel Diagnosis
        {
            get
            {
                return _diagnosis;
            }
            set
            {
                if (_diagnosis != value)
                {
                    _diagnosis = value;

                    OnPropertyChanged(() => Diagnosis);
                }
            }
        }
        public bool IsSelectingSymptomsActive
        {
            get
            {
                return _selectingSymptomsActive;
            }
            set
            {
                if (_selectingSymptomsActive != value)
                {
                    _selectingSymptomsActive = value;
                    OnPropertyChanged(() => IsSelectingSymptomsActive);
                }
            }
        }

        public void Subscribe()
        {
            msgHandlers = new List<EventMessageHandler>()
            {
                this.Subscribe((int)EventID.SymptomCheckedChanged, (e) =>
                {
                    var symptom = e.GetValue<SymptomViewModel>(Messages.Symptom);
                    var isChecked = e.GetValue<bool>(Messages.CheckedState);

                    OnSymptomCheckedChanged(symptom, isChecked);
                }),
                this.Subscribe((int)EventID.DiagnosisCheckedChanged, (e) =>
                {
                    var diagnosis = e.GetValue<DiagnosisViewModel>(Messages.Diagnosis);
                    var isChecked = e.GetValue<bool>(Messages.CheckedState);

                    OnDiagnosisCheckedChanged(diagnosis, isChecked);
                })
            };
        }

        public void Unsubscribe()
        {
            foreach (var h in msgHandlers)
            {
                h.Dispose();
            }
        }

        public void MakeCurrent()
        {
            current = this;

            EntityManagers.SymptomsManager.CheckThese(Symptoms);
            if (Diagnosis != null)
                EntityManagers.DiagnosisManager.Check(Diagnosis);
        }

        public HealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            this.healthRecord = hr;

            Symptoms = new ObservableCollection<SymptomViewModel>(
                EntityManagers.SymptomsManager.GetHealthRecordSymptoms(healthRecord));
            Diagnosis = EntityManagers.DiagnosisManager.GetHealthRecordDiagnosis(healthRecord);

            IsSelectingSymptomsActive = Diagnosis == null;

            Subscribe();
        }

        private void OnSymptomCheckedChanged(SymptomViewModel symptomVM, bool isChecked)
        {
            if (this == current)
            {
                if (isChecked)
                {
                    if (!Symptoms.Contains(symptomVM))
                    {
                        Symptoms.Add(symptomVM);
                        healthRecord.AddSymptom(symptomVM.symptom);
                    }
                }
                else
                {
                    Symptoms.Remove(symptomVM);
                    healthRecord.RemoveSymptom(symptomVM.symptom);
                }
                Symptoms = new ObservableCollection<SymptomViewModel>(Symptoms.OrderBy(s => s.SortingOrder));
                OnPropertyChanged(() => Symptoms);
                OnPropertyChanged(() => Name);
            }
        }

        private void OnDiagnosisCheckedChanged(DiagnosisViewModel diagnosisVM, bool isChecked)
        {
            if (this == current)
            {
                if (isChecked)
                {
                    Diagnosis = diagnosisVM;
                    healthRecord.Diagnosis = diagnosisVM.diagnosis;
                }
                else
                {
                    Diagnosis = null;
                    healthRecord.Diagnosis = null;
                }
                OnPropertyChanged(() => Name);
            }
        }
    }
}