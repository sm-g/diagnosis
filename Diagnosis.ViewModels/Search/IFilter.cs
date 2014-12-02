using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Search
{
    internal interface IFilter<T>
    {
        event EventHandler Cleared;
        event EventHandler Filtered;

        string Query { get; set; }

        ObservableCollection<T> Results { get; }

        bool UpdateResultsOnQueryChanges { get; set; }

        bool IsQueryEmpty { get; }

        ICommand ClearCommand { get; }

        void Clear();
        void Filter();
    }
}