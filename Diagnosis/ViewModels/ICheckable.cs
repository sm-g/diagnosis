using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public interface ICheckable
    {
        bool IsChecked { get; set; }

        bool IsNonCheckable { get; set; }

        ICommand ToggleCommand { get; }
    }
}