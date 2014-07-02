using System;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    interface IAutoComplete
    {
        char DelimSpacer { get; }
        ICommand EnterCommand { get; }
        event EventHandler<AutoCompleteEventArgs> SuggestionAccepted;
    }
}
