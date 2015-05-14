using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class SpecialityViewModel : ViewModelBase, IExistTestable
    {
        internal readonly Speciality spec;

        public SpecialityViewModel(Speciality s)
        {
            Contract.Requires(s != null);
            spec = s;
            this.validatableEntity = spec;
            spec.PropertyChanged += spec_PropertyChanged;
            spec.BlocksChanged += spec_BlocksChanged;
            spec.VocsChanged += spec_VocsChanged;

            var blockVms = spec.IcdBlocks.Select(x => new DiagnosisViewModel(x));
        }

        #region Model

        public string Title
        {
            get { return spec.Title; }
            set { spec.Title = value; }
        }

        #endregion Model

        public bool Unsaved
        {
            get { return spec.IsDirty; }
        }

        public bool HasExistingValue { get; set; }

        public bool WasEdited { get; set; }
        string[] IExistTestable.TestExistingFor
        {
            get { return new[] { "Title" }; }
        }
        string IExistTestable.ThisValueExistsMessage
        {
            get { return "Такая специальность уже есть."; }
        }

        public override string ToString()
        {
            return spec.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                spec.PropertyChanged -= spec_PropertyChanged;
                spec.BlocksChanged -= spec_BlocksChanged;
                spec.VocsChanged -= spec_VocsChanged;
            }
            base.Dispose(disposing);
        }

        private void spec_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            WasEdited = true;
        }

        private void spec_BlocksChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            WasEdited = true;

        }

        void spec_VocsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            WasEdited = true;
        }

    }
}