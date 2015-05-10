using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;

using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CriterionViewModel : ViewModelBase
    {
        internal readonly Criterion crit;

        public CriterionViewModel(Criterion cr)
        {
            Contract.Requires(cr != null);

            crit = cr;
            validatableEntity = crit;
            crit.PropertyChanged += model_PropertyChanged;
        }

        public string Description
        {
            get
            {
                return crit.Description;
            }
            set
            {
                crit.Description = value;
            }
        }

        public string Code
        {
            get
            {
                return crit.Code;
            }
            set
            {
                crit.Code = value;
            }
        }

        public string Value
        {
            get
            {
                return crit.Value;
            }
            set
            {
                crit.Value = value;
            }
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                crit.PropertyChanged -= model_PropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}