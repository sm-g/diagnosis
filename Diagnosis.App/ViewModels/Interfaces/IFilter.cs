namespace Diagnosis.App.ViewModels
{
    internal interface IFilter<T>
    {
        string Query { get; set; }

        System.Collections.ObjectModel.ObservableCollection<T> Results { get; }

        bool UpdateResultsOnQueryChanges { get; set; }

        bool IsQueryEmpty { get; }

        System.Windows.Input.ICommand ClearCommand { get; }

        void Clear();
    }
}