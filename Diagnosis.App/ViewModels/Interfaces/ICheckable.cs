using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public interface ICheckable
    {
        bool IsChecked { get; set; }

        bool IsSelected { get; set; }

        bool IsNonCheckable { get; set; }

        ICommand ToggleCommand { get; }

        void OnCheckedChanged();
    }
}