using EventAggregator;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public class DiagnosisViewModel : HierarchicalCheckable<DiagnosisViewModel>
    {
        internal readonly Diagnosis.Models.Diagnosis diagnosis;

        private PopupSearch<DiagnosisViewModel> _search;

        public IEditable Editable { get; private set; }

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

        public PopupSearch<DiagnosisViewModel> Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new PopupSearch<DiagnosisViewModel>(new DiagnosisSearcher(this, new SimpleSearcherSettings()));
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }

        private void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            this.AddIfNotExists(Search.SelectedItem, Search.searcher.AllChildren);
            Search.SelectedItem.IsChecked = true;
            Search.Clear();
        }

        public DiagnosisViewModel(Diagnosis.Models.Diagnosis d)
        {
            Contract.Requires(d != null);
            diagnosis = d;

            Editable = new EditableBase(this);

            ChildrenChanged += DiagnosisViewModel_ChildrenChanged;
        }

        private void DiagnosisViewModel_ChildrenChanged(object sender, System.EventArgs e)
        {
            IsNonCheckable = !IsTerminal;
        }

        public override string ToString()
        {
            return SearchText;
        }
    }
}