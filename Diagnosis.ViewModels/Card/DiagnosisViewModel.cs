using EventAggregator;
using System.Diagnostics.Contracts;
using System;

namespace Diagnosis.ViewModels
{
    public class DiagnosisViewModel : HierarchicalBase<DiagnosisViewModel>
    {
        internal readonly Diagnosis.Models.Diagnosis diagnosis;

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

        public DiagnosisViewModel(Diagnosis.Models.Diagnosis d)
        {
            Contract.Requires(d != null);
            this.diagnosis = d;

            ChildrenChanged += (s, e) =>
            {
                IsNonCheckable = !IsTerminal;

            };
        }

        public override string ToString()
        {
            return diagnosis.ToString();
        }

    }
}