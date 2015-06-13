using Diagnosis.Models;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels
{
    public class DiagnosisViewModel : HierarchicalBase<DiagnosisViewModel>
    {
        private readonly IIcdEntity _icd;

        public DiagnosisViewModel(IIcdEntity icd)
        {
            Contract.Requires(icd != null);
            _icd = icd;

            ChildrenChanged += (s, e) =>
            {
                IsNonCheckable = !IsTerminal;
            };
        }

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
                return _icd.Title;
            }
        }

        public string Code
        {
            get
            {
                return _icd.Code;
            }
        }

        /// <summary>
        /// Выбранное пользоваетелем <see cref="IsExpanded"/>
        /// </summary>
        public bool? UserExpaneded { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", Icd.Code, Icd.Title);
        }
    }
}