using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;

using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CriteriaGroupViewModel : ViewModelBase, IExistTestable
    {
        internal readonly CriteriaGroup critgr;

        public CriteriaGroupViewModel(CriteriaGroup crg)
        {
            Contract.Requires(crg != null);

            critgr = crg;
            validatableEntity = critgr;

            critgr.PropertyChanged += model_PropertyChanged;
        }

        public string Description { get { return critgr.Description; } set { critgr.Description = value; } }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            WasEdited = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                critgr.PropertyChanged -= model_PropertyChanged;
            }
            base.Dispose(disposing);
        }

        public bool HasExistingValue { get; set; }

        public bool WasEdited { get; set; }

        string[] IExistTestable.TestExistingFor
        {
            get { return new[] { "Description" }; }
        }
        string IExistTestable.ThisValueExistsMessage
        {
            get { return "Такая группа уже есть."; }
        }

    }
}