using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Controls
{
    /// <summary>
    /// Показывает результаты при установке фокуса.
    /// </summary>
    public class PopupSearchViewModel<T> : ViewModelBase
        where T : class
    {
        #region Fields
        internal readonly Func<string, IEnumerable<T>> searcher;
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(PopupSearchViewModel<>));
        private int _selectedIndex = -1;
        private bool _searchFocused;
        private bool _isResultsVisible;

        #endregion Fields

        public PopupSearchViewModel(Func<string, IEnumerable<T>> searcher)
        {
            this.searcher = searcher;
            Filter = new FilterViewModel<T>(searcher);
            Filter.Filtered += (s, e) =>
            {
                logger.DebugFormat("filtered in popupsearch, results: {0}", Filter.Results.Count);
                if (Filter.Results.Count > 0)
                    SelectedIndex = 0;
                IsResultsVisible = true;
            };

            // Filter.Clear(); // no results made here
        }

        public event EventHandler<ObjectEventArgs> ResultItemSelected;
        public FilterViewModel<T> Filter
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
                if (_isResultsVisible != value)
                {
                    _isResultsVisible = value;
                    logger.DebugFormat("IsResultsVisible {0}", value);

                    OnPropertyChanged(() => IsResultsVisible);
                }
            }
        }

        public bool HideResultsAfterSelected { get; set; }

        /// <summary>
        /// public для выбора мышью (dynamic) и
        /// для выбора элемента, который не совпадает с SelectedItem (SearchTree).
        /// </summary>
        public void RaiseResultItemSelected(object realItem)
        {
            // logger.DebugFormat("raise");

            var h = ResultItemSelected;
            if (h != null)
            {
                h(this, new ObjectEventArgs(realItem));
            }

            if (HideResultsAfterSelected)
                IsResultsVisible = false;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    Filter.Dispose();
                }

            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }



}