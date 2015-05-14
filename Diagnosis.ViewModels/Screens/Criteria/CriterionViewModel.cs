using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;

using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CriterionViewModel : ViewModelBase, IExistTestable
    {
        internal readonly Criterion crit;

        public CriterionViewModel(Criterion cr)
        {
            Contract.Requires(cr != null);

            crit = cr;
            validatableEntity = crit;
            crit.PropertyChanged += model_PropertyChanged;
        }

        public string Description { get { return crit.Description; } set { crit.Description = value; } }

        public string Code { get { return crit.Code; } set { crit.Code = value; } }

        public string Value { get { return crit.Value; } set { crit.Value = value; } }

        public bool HasExistingValue { get; set; }

        public bool WasEdited { get; set; }

        string[] IExistTestable.TestExistingFor
        {
            get { return new[] { "Code" }; }
        }
        string IExistTestable.ThisValueExistsMessage
        {
            get { return "Такой критерий уже есть."; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                crit.PropertyChanged -= model_PropertyChanged;
            }
            base.Dispose(disposing);
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            WasEdited = true;
        }
    }
}