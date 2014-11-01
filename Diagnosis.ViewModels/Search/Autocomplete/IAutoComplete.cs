using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public interface IAutoComplete
    {
        char DelimSpacer { get; }
        ICommand EnterCommand { get; }
        event EventHandler<AutoCompleteEventArgs> SuggestionAccepted;
    }
    public class AutoCompleteEventArgs : EventArgs
    {
        public readonly object item;
        [DebuggerStepThrough]
        public AutoCompleteEventArgs(object item)
        {
            this.item = item;
        }
    }
}
