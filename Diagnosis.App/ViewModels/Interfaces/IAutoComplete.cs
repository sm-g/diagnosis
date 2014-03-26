using System;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    interface IAutoComplete
    {
        string FullString { get; set; }
        void Reset();
        ICommand EnterCommand { get; }
        event EventHandler SuggestionAccepted;
    }
}
