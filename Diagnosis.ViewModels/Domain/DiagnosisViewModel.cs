using EventAggregator;
using System.Diagnostics.Contracts;
using System;

namespace Diagnosis.ViewModels
{
    public class DiagnosisViewModel : HierarchicalBase<DiagnosisViewModel>
    {
        internal readonly Diagnosis.Models.Diagnosis diagnosis;
        
        public Editable Editable { get; private set; }

        public string Name
        {
            get
            {
                return diagnosis.Title;
            }
            set
            {
                if (diagnosis.Title != value)
                {
                    diagnosis.Title = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string SearchText
        {
            get
            {
                return Code + ' ' + Name;
            }
        }

        public string Code
        {
            get
            {
                return diagnosis.Code;
            }
            set
            {
                if (diagnosis.Code != value)
                {
                    diagnosis.Code = value;
                    OnPropertyChanged("Code");
                }
            }
        }

        public void Unsubscribe()
        {
            ChildrenChanged -= DiagnosisViewModel_ChildrenChanged;
        }
        
        public DiagnosisViewModel(Diagnosis.Models.Diagnosis d)
        {
            Contract.Requires(d != null);
            this.diagnosis = d;

            Editable = new Editable(diagnosis);

            ChildrenChanged += DiagnosisViewModel_ChildrenChanged;
        }

        private void DiagnosisViewModel_ChildrenChanged(object sender, HierarchicalEventAgrs<DiagnosisViewModel> e)
        {
            IsNonCheckable = !IsTerminal;
        }

        public override string ToString()
        {
            return diagnosis.ToString();
        }

    }
}