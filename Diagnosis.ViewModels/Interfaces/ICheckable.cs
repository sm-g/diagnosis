using System;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public interface ICheckable
    {
        event EventHandler<CheckableEventArgs> CheckedChanged;
        event EventHandler<CheckableEventArgs> SelectedChanged;

        bool IsChecked { get; set; }

        bool IsSelected { get; set; }

        bool IsNonCheckable { get; set; }

        ICommand ToggleCommand { get; }
    }
}