using Diagnosis.Models;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels.Search.Autocomplete
{
    public class DiagnosisViewModel : HierarchicalBase<DiagnosisViewModel>
    {
        internal readonly Diagnosis.Models.Diagnosis diagnosis;
        private readonly IIcdEntity _icd;

        public IIcdEntity Icd
        {
            get
            {
                return _icd;
            }
        }

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

        public DiagnosisViewModel(IIcdEntity icd)
        {
            Contract.Requires(icd != null);
            _icd = icd;

            ChildrenChanged += (s, e) =>
            {
                IsNonCheckable = !IsTerminal;
            };
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Icd.Code, Icd.Title);
        }
    }
}