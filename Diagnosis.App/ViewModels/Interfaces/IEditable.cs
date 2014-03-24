using System;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public interface IEditable
    {
        event EventHandler Committed;
        event EventHandler Deleted;
        event EventHandler ModelPropertyChanged;

        ICommand CommitCommand { get; }

        ICommand DeleteCommand { get; }

        ICommand EditCommand { get; }

        ICommand RevertCommand { get; }

        bool IsEditorActive { get; set; }

        bool IsEditorFocused { get; set; }

        bool IsReady { get; }

        bool IsDirty { get; }

        string Name { get; set; }

        void MarkDirty();
    }
}