using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Diagnosis.ViewModels
{
    public interface IHierarchical<T> where T : class
    {
        bool IsRoot { get; }

        bool IsTerminal { get; }

        string Name { get; set; }

        T Parent { get; }

        ObservableCollection<T> Children { get; }

        ObservableCollection<T> TerminalChildren { get; }

        ObservableCollection<T> NonTerminalChildren { get; }

        IEnumerable<T> AllChildren { get; }

        T Add(T symptomVM);

        T AddIfNotExists(T vm, bool checkAllChildren);

        T Remove(T symptomVM);

        void Delete();
    }
}