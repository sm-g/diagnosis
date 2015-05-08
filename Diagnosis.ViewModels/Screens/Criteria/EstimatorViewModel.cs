using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;

using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class EstimatorViewModel : ViewModelBase
    {
        internal readonly Estimator est;

        public EstimatorViewModel(Estimator e)
        {
            Contract.Requires(e != null);

            est = e;
            validatableEntity = est;
            est.PropertyChanged += model_PropertyChanged;
        }

        public string Description
        {
            get
            {
                return est.Description;
            }
            set
            {
                est.Description = value;
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
                est.PropertyChanged -= model_PropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}