using EventAggregator;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public class DiagnosisViewModel : HierarchicalCheckable<DiagnosisViewModel>
    {
        internal readonly Diagnosis.Models.Diagnosis diagnosis;

        private DiagnosisSearch _search;

        public IEditable Editable { get; private set; }

        public string SortingOrder { get; private set; }

        public string Name
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

        public string SearchText
        {
            get
            {
                return Code + ' ' + Name;
            }
        }

        public string Code
        {
            get
            {
                return diagnosis.Code;
            }
            set
            {
                if (diagnosis.Code != value)
                {
                    diagnosis.Code = value;
                    OnPropertyChanged(() => Code);
                }
            }
        }

        public override void OnCheckedChanged()
        {
            base.OnCheckedChanged();
            this.Send((int)EventID.DiagnosisCheckedChanged, new DiagnosisCheckedChangedParams(this, checkable.IsChecked).Params);
        }

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
            Search.SelectedItem.checkable.IsChecked = true;
            Search.Clear();
        }

        public DiagnosisViewModel(Diagnosis.Models.Diagnosis d)
        {
            Contract.Requires(d != null);
            diagnosis = d;

            Editable = new EditableBase(this);

            ChildrenChanged += DiagnosisViewModel_ChildrenChanged;
        }

        public DiagnosisViewModel(string title)
            : this(new Diagnosis.Models.Diagnosis("code", title))
        {
        }

        private void DiagnosisViewModel_ChildrenChanged(object sender, System.EventArgs e)
        {
            IsNonCheckable = !IsTerminal;
        }

        internal void Initialize()
        {
            int i = 1;
            foreach (DiagnosisViewModel child in Children)
            {
                child.SortingOrder = this.SortingOrder + i++;
                child.Initialize();
            }
        }

        public override string ToString()
        {
            return SearchText;
        }
    }
}