using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class HierarchicalExplorer<T> : ViewModelBase, ISearchable where T : HierarchicalBase<T>, ISearchable
    {
        private RelayCommand<T> _clickItem;
        private T _current;
        private RelayCommand _goUp;

        public HierarchicalExplorer(IList<T> items)
        {
            Contract.Requires(items != null);
            Contract.Requires(items.Count > 0);

            Items = new ObservableCollection<T>(items);
            CurrentItem = items[0].Parent;
        }

        public ObservableCollection<T> Items
        {
            get;
            private set;
        }

        public T CurrentItem
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

                    Items = _current.Children;

                    DirtySearchCreation();

                    OnPropertyChanged(() => CurrentItem);
                    OnPropertyChanged(() => Items);

                    CreateBreadcrumbs();
                }
            }
        }

        private void DirtySearchCreation()
        {
            // TODO
        }

        private void CreateBreadcrumbs()
        {
            var list = new List<T>();
            T s = CurrentItem;
            while (!s.IsRoot)
            {
                list.Add(s);
                s = s.Parent;
            }
            list.Reverse();
            Breadcrumbs = new ObservableCollection<T>(list);

            OnPropertyChanged(() => Breadcrumbs);
        }

        public ObservableCollection<T> Breadcrumbs
        {
            get;
            private set;
        }

        public RelayCommand GoUp
        {
            get
            {
                return _goUp ?? (_goUp = new RelayCommand(
                        () => CurrentItem = CurrentItem.Parent,
                        () => CurrentItem != null && !CurrentItem.IsRoot
                        ));
            }
        }

        public RelayCommand<T> ClickItem
        {
            get
            {
                return _clickItem
                    ?? (_clickItem = new RelayCommand<T>(
                                          p =>
                                          {
                                              CurrentItem = p;
                                          },
                                          p => p != null && !p.IsTerminal
                                          ));
            }
        }


        #region ISearchable

        private ICommand _searchCommand;
        private bool _searchActive;
        private bool _searchFocused;

        public string Representation
        {
            get
            {
                return CurrentItem.Name;
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
                if (_searchActive != value)
                {
                    _searchActive = value;
                    IsSearchFocused = value;

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


        private HierarchicalSearch<T> _search;

        public HierarchicalSearch<T> Search
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

        private void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            CurrentItem.AddIfNotExists(Search.SelectedItem, Search.AllChildren);
            Search.SelectedItem.IsChecked = true;
            Search.Clear();
        }
    }
}