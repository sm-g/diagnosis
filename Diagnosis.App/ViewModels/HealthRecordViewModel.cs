using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using System.Linq;
using EventAggregator;

namespace Diagnosis.App.ViewModels
{
    public class HealthRecordViewModel : CheckableBase
    {
        private HealthRecord healthRecord;
        List<EventMessageHandler> msgHandlers;

        #region CheckableBase

        public override string Name
        {
            get
            {
                return string.Concat(Symptoms.OrderBy(s => s.Priority).Select(s => s.Name + " "));
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


        private static HealthRecordViewModel current;

        public HealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            this.healthRecord = hr;
            Symptoms = new ObservableCollection<SymptomViewModel>(
                EntityManagers.SymptomsManager.GetHealthRecordSymptoms(healthRecord));
            Subscribe();
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
            }
        }

    }
}