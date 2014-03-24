using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public interface ISearchCommon
    {
        event EventHandler ResultItemSelected;

        string Query { get; set; }
        int SelectedIndex { get; set; }
        ICommand ClearCommand { get; }
        ICommand SearchCommand { get; }
        ICommand SelectCommand { get; }
        bool IsSearchActive { get; set; }
        bool IsSearchFocused { get; set; }

        void Clear();
        void RaiseResultItemSelected();
    }
}
