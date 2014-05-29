using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class HierarchicalExplorer<T> : ViewModelBase where T : HierarchicalCheckable<T>
    {
        private RelayCommand<T> _clickItem;
        private T _current;
        private RelayCommand _goUp;
        private PopupSearch<T> _search;
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

                    OnPropertyChanged(() => CurrentItem);
                    OnPropertyChanged(() => Items);

                    CreateBreadcrumbs();
                }
            }
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
        public PopupSearch<T> Search
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

        public HierarchicalExplorer(IList<T> items)
        {
            Contract.Requires(items != null);
            Contract.Requires(items.Count > 0);

            Items = new ObservableCollection<T>(items);
            CurrentItem = items[0].Parent;
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

        private void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            CurrentItem.AddIfNotExists(Search.SelectedItem, Search.searcher.AllChildren);
            Search.SelectedItem.IsChecked = true;
            Search.Clear();
        }
    }
}