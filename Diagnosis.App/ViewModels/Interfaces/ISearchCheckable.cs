using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public interface ISearchCheckable<T> : ISearchCommon where T : ICheckable
    {
        ObservableCollection<T> Results { get; }
        T SelectedItem { get; }
        bool WithNonCheckable { get; set; }
        bool WithChecked { get; set; }
    }
}