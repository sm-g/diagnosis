using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class HealthRecordViewModel : CheckableBase
    {
        private HealthRecord healthRecord;

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

        public HealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            this.healthRecord = hr;
            Symptoms = new ObservableCollection<SymptomViewModel>();
        }
    }
}