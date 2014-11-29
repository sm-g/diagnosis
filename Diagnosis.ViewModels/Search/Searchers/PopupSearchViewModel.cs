using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Search
{
    public class PopupSearchViewModel<T> : ViewModelBase where T : class
    {
        #region Fields

        internal readonly Func<string, IEnumerable<T>> searcher;

        private int _selectedIndex = -1;
        private bool _searchFocused;
        private bool _isResultsVisible;

        #endregion Fields

        public event EventHandler<ObjectEventArgs> ResultItemSelected;

        public NewFilterViewModel<T> Filter
        {
            get;
            private set;
        }

        public T SelectedItem
        {
            get
            {
                if (SelectedIndex > -1 && SelectedIndex < Filter.Results.Count)
                    return Filter.Results[SelectedIndex];
                else
                    return null;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                    OnPropertyChanged("SelectedItem");
                }
            }
        }

        public ICommand ToggleResultsVisibleCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            IsResultsVisible = !IsResultsVisible;
                        }
                       );
            }
        }

        public ICommand SelectCommand
        {
            get
            {
                return new RelayCommand(
                    () => RaiseResultItemSelected(SelectedItem));
            }
        }

        public bool IsFocused
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
                    if (value)
                    {
                        IsResultsVisible = true;
                    }
                    OnPropertyChanged("IsFocused");
                }
            }
        }

        public bool IsResultsVisible
        {
            get
            {
                return _isResultsVisible;
            }
            set
            {
                // set to true only if IsFocused
                if (_isResultsVisible != value && (value == IsFocused || IsFocused))
                {
                    _isResultsVisible = value;

                    OnPropertyChanged("IsResultsVisible");
                }
            }
        }

        /// <summary>
        /// Для выбора элемента, который не совпадает с SelectedItem (SearchTree).
        /// </summary>
        public void SelectReal(object item)
        {
            T asT = item as T;
            if (asT == null)
            {
                throw new ArgumentException("Selected item type is wrong.");
            }

            RaiseResultItemSelected(asT);
        }

        public void RaiseResultItemSelected(T item) // public for selecting by mouse in FloatSearch (dynamic)
        {
            var h = ResultItemSelected;
            if (h != null)
            {
                h(this, new ObjectEventArgs(item));
            }

            IsResultsVisible = false;
        }

        public PopupSearchViewModel(Func<string, IEnumerable<T>> searcher)
        {
            this.searcher = searcher;
            Filter = new NewFilterViewModel<T>(searcher);
            Filter.Filtered += (s, e) =>
            {
                if (Filter.Results.Count > 0)
                    SelectedIndex = 0;
                IsResultsVisible = true;
            };

            Filter.Clear(); // no results made here
        }

        [Serializable]
        public class ObjectEventArgs : EventArgs
        {
            public readonly T arg;

            [System.Diagnostics.DebuggerStepThrough]
            public ObjectEventArgs(T arg)
            {
                this.arg = arg;
            }
        }
    }



}