﻿using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class SpecialityViewModel : ViewModelBase
    {
        internal readonly Speciality spec;

        public SpecialityViewModel(Speciality s)
        {
            Contract.Requires(s != null);
            spec = s;
            spec.PropertyChanged += spec_PropertyChanged;
            spec.BlocksChanged += spec_BlocksChanged;

            var blockVms = spec.IcdBlocks.Select(x => new DiagnosisViewModel(x));
            Blocks = new ObservableCollection<DiagnosisViewModel>(blockVms);
            SelectedBlocks = new ObservableCollection<DiagnosisViewModel>();
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

        public bool HasExistingTitle { get; set; }

        public bool WasEdited { get; set; }

        public ObservableCollection<DiagnosisViewModel> Blocks { get; private set; }

        public ObservableCollection<DiagnosisViewModel> SelectedBlocks { get; set; }

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
                if (HasExistingTitle) message = "Такая специальность уже есть.";
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
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (IcdBlock item in e.NewItems)
                {
                    var vm = new DiagnosisViewModel(item);
                    Blocks.Add(vm);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (IcdBlock item in e.OldItems)
                {
                    var vm = Blocks.Where(w => w.Icd as IcdBlock == item).FirstOrDefault();
                    Blocks.Remove(vm);
                }
            }
        }
    }
}