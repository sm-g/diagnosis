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

        public override string this[string columnName]
        {
            get
            {
                if (!WasEdited) return string.Empty;

                var results = spec.SelfValidate();
                if (results == null)
                    return string.Empty;
                var message = results.Errors
                    .Where(x => x.PropertyName == columnName)
                    .Select(x => x.ErrorMessage)
                    .FirstOrDefault();
                if (HasExistingValue) message = "Такая специальность уже есть.";
                return message != null ? message : string.Empty;
            }
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