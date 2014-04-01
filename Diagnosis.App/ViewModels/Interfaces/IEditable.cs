using System;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public delegate void EditableEventHandler(object sender, EditableEventArgs e);

    public class EditableEventArgs : EventArgs
    {
        public ViewModelBase viewModel;
        public EditableEventArgs(ViewModelBase vm)
        {
            viewModel = vm;
        }
    }

    public interface IEditable
    {
        event EditableEventHandler Committed;
        event EditableEventHandler Deleted;
        event EditableEventHandler ModelPropertyChanged;

        ICommand CommitCommand { get; }
        ICommand DeleteCommand { get; }
        ICommand EditCommand { get; }
        ICommand RevertCommand { get; }

        bool IsEditorActive { get; set; }
        bool IsEditorFocused { get; set; }

        bool SwitchedOn { get; set; }

        bool IsDirty { get; }

        void MarkDirty();
    }
}