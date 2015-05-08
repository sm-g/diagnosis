using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;

using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CriteriaGroupViewModel : ViewModelBase
    {
        internal readonly CriteriaGroup critgr;

        public CriteriaGroupViewModel(CriteriaGroup crg)
        {
            Contract.Requires(crg != null);

            critgr = crg;
            validatableEntity = critgr;

            critgr.PropertyChanged += model_PropertyChanged;
        }

        public string Description
        {
            get
            {
                return critgr.Description;
            }
            set
            {
                critgr.Description = value;
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
                critgr.PropertyChanged -= model_PropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}