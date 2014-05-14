using System;
using System.ComponentModel;
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

    public interface IEditable : INotifyPropertyChanged
    {
        event EditableEventHandler Committed;
        event EditableEventHandler Reverted;
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
        bool IsEmpty { get; }

        bool CanBeDeleted { get; set; }
        bool CanBeDirty { get; set; }

        void MarkDirty();
    }
}