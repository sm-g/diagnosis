using Diagnosis.Models;
using EventAggregator;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public class SymptomViewModel : HierarchicalCheckable<SymptomViewModel>
    {
        internal readonly Symptom symptom;

        private SymptomSearch _search;

        public EditableBase Editable { get; private set; }

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
                    Editable.MarkDirty();
                }
            }
        }

        public string Name
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
                    Editable.MarkDirty();
                }
            }
        }

        public override void OnCheckedChanged()
        {
            base.OnCheckedChanged();

            this.Send((int)EventID.SymptomCheckedChanged, new SymptomCheckedChangedParams(this, IsChecked).Params);
        }

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

            Editable = new EditableBase(this);
        }

        public SymptomViewModel(string title)
            : this(new Symptom(title))
        {
        }

        private void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            this.AddIfNotExists(Search.SelectedItem, Search.AllChildren);
            Search.SelectedItem.checkable.IsChecked = true;
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