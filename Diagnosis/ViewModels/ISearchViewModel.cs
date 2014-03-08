using System;

namespace Diagnosis.ViewModels
{
    public interface ISearchViewModel<T>
     where T : class, ISearchable
    {
        void Clear();
        System.Windows.Input.ICommand ClearCommand { get; }
        string Query { get; set; }
        System.Collections.ObjectModel.ObservableCollection<T> Results { get; }
        int ResultsCount { get; }
        int SelectedIndex { get; set; }
        T SelectedItem { get; }
    }
}
