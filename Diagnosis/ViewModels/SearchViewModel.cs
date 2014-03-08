using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public abstract class SearchViewModel<T> : ViewModelBase, ISearchViewModel<T> where T : class, ISearchable
    {
        private string _query;
        private ICommand _clear;
        private int _selectedIndex = -1;

        public event EventHandler ResultItemSelected;

        public ObservableCollection<T> Results { get; protected set; }

        public string Query
        {
            get
            {
                return _query;
            }
            set
            {
                if (_query != value)
                {
                    _query = value;
                    OnPropertyChanged(() => Query);
                }
                MakeResults(_query);
            }
        }

        public T SelectedItem
        {
            get
            {
                if (SelectedIndex != -1)
                    return Results[SelectedIndex];
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
                    OnPropertyChanged(() => SelectedIndex);
                    OnPropertyChanged(() => SelectedItem);
                }
            }
        }

        public int ResultsCount
        {
            get
            {
                return Results.Count;
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return _clear ?? (_clear = new RelayCommand(Clear, () => Query != ""));
            }
        }

        public void Clear()
        {
            Query = "";
        }

        public void RaiseResultItemSelected()
        {
            var h = ResultItemSelected;
            if (h != null)
            {
                h(this, new EventArgs());
            }
        }

        protected virtual void MakeResults(string query)
        {
            Contract.Requires(query != null);
        }

        public SearchViewModel()
        {
        }
    }
}