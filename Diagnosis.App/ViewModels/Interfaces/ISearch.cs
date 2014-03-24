using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public interface ISearch<T> : ISearchCommon
    {
        ObservableCollection<T> Results { get; }
        T SelectedItem { get; }
    }
}