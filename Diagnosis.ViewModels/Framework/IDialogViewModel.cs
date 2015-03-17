using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public interface IDialogViewModel
    {
        bool? DialogResult { get; }
        string Title { get; }
        string HelpTopic { get; }
        bool WithHelpButton { get; }
        ICommand OkCommand { get; }
        ICommand ApplyCommand { get; }
        ICommand CancelCommand { get; }
        bool CanOk { get; }
        bool CanApply { get; }
    }
}
