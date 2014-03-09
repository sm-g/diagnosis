using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Collections.Generic;

namespace Diagnosis.ViewModels
{
    public class SymptomViewModel : CheckableHierarchicalBase<SymptomViewModel>, ISearchable
    {
        Symptom symptom;

        private SymptomSearch _search;

        public string SortingOrder { get; private set; }


        public int Level
        {
            get
            {
                return symptom.Level;
            }
            set
            {
                if (symptom.Level != value)
                {
                    symptom.Level = value;
                    OnPropertyChanged(() => Level);
                }
            }
        }
        #region IHierarchical

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

        #endregion

        #region ICheckableHierarchical

        protected override void OnCheckedChanged()
        {
            this.Send((int)EventID.SymptomCheckedChanged, new SymptomCheckedChangedParams(this, IsChecked).Params);
        }

        #endregion

        #region ISearchable

        public string Representation
        {
            get
            {
                return Name;
            }
        }

        #endregion

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
            : this(new Symptom() { Title = title })
        {
        }

        internal SymptomViewModel()
            : this(new Symptom())
        {
        }

        void _search_ResultItemSelected(object sender, System.EventArgs e)
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