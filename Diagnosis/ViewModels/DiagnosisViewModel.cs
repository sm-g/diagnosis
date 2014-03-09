using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Collections.Generic;

namespace Diagnosis.ViewModels
{
    public class DiagnosisViewModel : CheckableHierarchicalBase<DiagnosisViewModel>, ISearchable
    {
        Diagnosis.Models.Diagnosis diagnosis;
        private DiagnosisSearch _search;

        public string SortingOrder { get; private set; }


        #region IHierarchical

        public override string Name
        {
            get
            {
                return diagnosis.Title;
            }
            set
            {
                if (diagnosis.Title != value)
                {
                    diagnosis.Title = value;
                    OnPropertyChanged(() => Name);
                }
            }
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

        #region ICheckableHierarchical

        protected override void OnCheckedChanged()
        {
            this.Send((int)EventID.DiagnosisCheckedChanged, new DiagnosisCheckedChangedParams(this, IsChecked).Params);

        }

        #endregion

        public DiagnosisSearch Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new DiagnosisSearch(this);
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }

        private void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            this.AddIfNotExists(Search.SelectedItem, Search.AllChildren);
            Search.SelectedItem.IsChecked = true;
            Search.Clear();
        }

        public DiagnosisViewModel(Diagnosis.Models.Diagnosis d)
        {
            Contract.Requires(d != null);
            diagnosis = d;
        }

        public DiagnosisViewModel(string title)
            : this(new Diagnosis.Models.Diagnosis() { Title = title })
        {
        }

        internal DiagnosisViewModel()
            : this(new Diagnosis.Models.Diagnosis())
        {
        }

        //void _search_ResultItemSelected(object sender, System.EventArgs e)
        //{
        //    CheckChild(Search.SelectedItem, Search.AllChildren);
        //    Search.Clear();
        //}

        internal void Initialize()
        {
            int i = 1;
            foreach (DiagnosisViewModel child in Children)
            {
                child.Parent = this;
                child.SortingOrder = this.SortingOrder + i++;
                child.Initialize();
            }
        }
    }
}