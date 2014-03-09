using System;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Diagnosis.ViewModels
{
    public interface ISearch<T>
     where T : ISearchable
    {
        event EventHandler ResultItemSelected;

        string Query { get; set; }
        ObservableCollection<T> Results { get; }
        int ResultsCount { get; }
        int SelectedIndex { get; set; }
        T SelectedItem { get; }

        void Clear();
        ICommand ClearCommand { get; }

        void RaiseResultItemSelected();
    }
}
