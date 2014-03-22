using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public interface ISearch<T>
    {
        event EventHandler ResultItemSelected;

        string Query { get; set; }
        ObservableCollection<T> Results { get; }
        int ResultsCount { get; }
        int SelectedIndex { get; set; }
        T SelectedItem { get; }
        ICommand ClearCommand { get; }
        ICommand SearchCommand { get; }
        bool IsSearchActive { get; set; }
        bool IsSearchFocused { get; set; }

        void Clear();
        void RaiseResultItemSelected();
    }
}