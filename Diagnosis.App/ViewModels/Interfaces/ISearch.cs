using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public interface ISearch<T> where T : class
    {
        event EventHandler ResultItemSelected;

        string Query { get; set; }
        ObservableCollection<T> Results { get; }
        T SelectedItem { get; }

        int SelectedIndex { get; set; }
        ICommand ClearCommand { get; }
        ICommand ToggleSearchActiveCommand { get; }
        ICommand SelectCommand { get; }
        bool IsSearchActive { get; set; }
        bool IsSearchFocused { get; set; }
        bool IsResultsVisible { get; }

        bool SwitchedOn { get; set; }

        void Clear();
        void RaiseResultItemSelected();
    }
}
