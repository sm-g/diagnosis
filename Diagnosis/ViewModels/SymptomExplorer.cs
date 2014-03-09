using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class SymptomExplorer : ViewModelBase
    {
        private RelayCommand<SymptomViewModel> _clickItem;
        private SymptomViewModel _current;
        private RelayCommand _goUp;

        public SymptomExplorer(IList<SymptomViewModel> symptoms)
        {
            Contract.Requires(symptoms != null);
            Contract.Requires(symptoms.Count > 0);

            Symptoms = new ObservableCollection<SymptomViewModel>(symptoms);
            CurrentSymptom = symptoms[0].Parent;
        }

        public ObservableCollection<SymptomViewModel> Symptoms
        {
            get;
            private set;
        }

        public SymptomViewModel CurrentSymptom
        {
            get
            {
                return _current;
            }
            set
            {
                if (_current != value)
                {
                    _current = value;

                    Symptoms = _current.Children;
                    Search = new SymptomSearch(_current);

                    OnPropertyChanged(() => CurrentSymptom);
                    OnPropertyChanged(() => Symptoms);

                    CreateBreadcrumbs();
                }
            }
        }

        private void CreateBreadcrumbs()
        {
            var list = new List<SymptomViewModel>();
            SymptomViewModel s = CurrentSymptom;
            while (!s.IsRoot)
            {
                list.Add(s);
                s = s.Parent;
            }
            list.Reverse();
            Breadcrumbs = new ObservableCollection<SymptomViewModel>(list);

            OnPropertyChanged(() => Breadcrumbs);
        }

        public ObservableCollection<SymptomViewModel> Breadcrumbs
        {
            get;
            private set;
        }

        public RelayCommand GoUp
        {
            get
            {
                return _goUp ?? (_goUp = new RelayCommand(
                        () => CurrentSymptom = CurrentSymptom.Parent,
                        () => CurrentSymptom != null && !CurrentSymptom.IsRoot
                        ));
            }
        }

        public RelayCommand<SymptomViewModel> ClickItem
        {
            get
            {
                return _clickItem
                    ?? (_clickItem = new RelayCommand<SymptomViewModel>(
                                          p =>
                                          {
                                              CurrentSymptom = p;
                                          },
                                          p => p != null && !p.IsTerminal
                                          ));
            }
        }


        private SymptomSearch _search;
        public SymptomSearch Search
        {
            get
            {
                return _search;
            }
            set
            {
                if (_search != value)
                {
                    if (_search != null)
                    {
                        _search.ResultItemSelected -= _search_ResultItemSelected;
                    }
                    _search = value;
                    _search.ResultItemSelected += _search_ResultItemSelected;
                    OnPropertyChanged(() => Search);
                }
            }
        }

        void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            CurrentSymptom.AddIfNotExists(Search.SelectedItem, Search.AllChildren);
            Search.SelectedItem.IsChecked = true;
            Search.Clear();
        }
    }
}