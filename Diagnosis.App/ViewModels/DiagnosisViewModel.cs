using EventAggregator;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class DiagnosisViewModel : HierarchicalBase<DiagnosisViewModel>
    {
        private Diagnosis.Models.Diagnosis diagnosis;
        private DiagnosisSearch _search;

        public string SortingOrder { get; private set; }

        public new bool IsNonCheckable
        {
            get
            {
                return !IsTerminal;
            }
            set
            {
            }
        }

        #region HierarchicalBase

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
            this.Send((int)EventID.DiagnosisCheckedChanged, new DiagnosisCheckedChangedParams(this, IsChecked).Params);
        }

        #endregion HierarchicalBase

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