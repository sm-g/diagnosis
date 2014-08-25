using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public interface ICheckable
    {
        event CheckableEventHandler CheckedChanged;
        event CheckableEventHandler SelectedChanged;

        bool IsChecked { get; set; }

        bool IsSelected { get; set; }

        bool IsNonCheckable { get; set; }

        ICommand ToggleCommand { get; }
    }
}