using Diagnosis.Models;
using EventAggregator;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class SymptomViewModel : HierarchicalBase<SymptomViewModel>, ISearchable
    {
        private Symptom symptom;

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
                return base.IsReady && !IsSearchActive;
            }
        }

        protected override void OnCheckedChanged()
        {
            base.OnCheckedChanged();

            this.Send((int)EventID.SymptomCheckedChanged, new SymptomCheckedChangedParams(this, IsChecked).Params);
        }

        #endregion HierarchicalBase

        #region ISearchable

        private ICommand _searchCommand;
        private bool _searchActive;
        private bool _searchFocused;

        public string Representation
        {
            get
            {
                return Name;
            }
        }

        public bool IsSearchActive
        {
            get
            {
                return _searchActive;
            }
            set
            {
                if (_searchActive != value && (IsReady || !value))
                {
                    _searchActive = value;
                    OnPropertyChanged(() => IsSearchActive);
                }
            }
        }

        public bool IsSearchFocused
        {
            get
            {
                return _searchFocused;
            }
            set
            {
                if (_searchFocused != value)
                {
                    _searchFocused = value;
                    OnPropertyChanged(() => IsSearchFocused);
                }
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand
                    ?? (_searchCommand = new RelayCommand(
                                          () =>
                                          {
                                              IsSearchActive = !IsSearchActive;
                                          }
                                          ));
            }
        }

        #endregion ISearchable

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