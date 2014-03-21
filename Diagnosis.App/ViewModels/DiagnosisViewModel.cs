using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;

namespace Diagnosis.ViewModels
{
    public class DiagnosisViewModel : HierarchicalBase<DiagnosisViewModel>, ISearchable
    {
        Diagnosis.Models.Diagnosis diagnosis;
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
                return base.IsReady && !IsSearchActive;
            }
        }

        protected override void OnCheckedChanged()
        {
            base.OnCheckedChanged();
            this.Send((int)EventID.DiagnosisCheckedChanged, new DiagnosisCheckedChangedParams(this, IsChecked).Params);
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