﻿using Diagnosis.App;
using Diagnosis.Models;
using EventAggregator;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class SymptomViewModel : HierarchicalBase<SymptomViewModel>
    {
        internal Symptom symptom;

        private SymptomSearch _search;

        public string SortingOrder { get; private set; }

        public byte Priority
        {
            get
            {
                return symptom.Priority;
            }
            set
            {
                if (symptom.Priority != value)
                {
                    symptom.Priority = value;
                    OnPropertyChanged(() => Priority);
                }
            }
        }

        #region HierarchicalBase

        public override string Name
        {
            get
            {
                return symptom.Title;
            }
            set
            {
                if (symptom.Title != value)
                {
                    symptom.Title = value;
                    OnPropertyChanged(() => Name);
                }
            }
        }

        public override bool IsReady
        {
            get
            {
                return base.IsReady;
            }
        }

        protected override void OnCheckedChanged()
        {
            base.OnCheckedChanged();

            this.Send((int)EventID.SymptomCheckedChanged, new SymptomCheckedChangedParams(this, IsChecked).Params);
        }

        #endregion HierarchicalBase

        public SymptomSearch Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new SymptomSearch(this);
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }

        public SymptomViewModel(Symptom s)
        {
            Contract.Requires(s != null);
            symptom = s;
        }

        public SymptomViewModel(string title)
            : this(new Symptom(title))
        {
        }

        private void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            this.AddIfNotExists(Search.SelectedItem, Search.AllChildren);
            Search.SelectedItem.IsChecked = true;
            Search.Clear();
        }

        internal void Initialize()
        {
            int i = 1;
            foreach (SymptomViewModel child in Children)
            {
                child.Parent = this;
                child.SortingOrder = this.SortingOrder + i++;
                child.Initialize();
            }
        }
    }
}