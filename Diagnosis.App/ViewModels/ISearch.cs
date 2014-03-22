using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
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