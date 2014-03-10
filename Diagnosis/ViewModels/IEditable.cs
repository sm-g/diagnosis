using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public interface IEditable
    {
        ICommand CommitCommand { get; }

        ICommand DeleteCommand { get; }

        ICommand EditCommand { get; }

        ICommand RevertCommand { get; }

        bool IsEditorActive { get; set; }

        bool IsEditorFocused { get; set; }

        bool IsReady { get; }

        string Name { get; set; }
    }
}