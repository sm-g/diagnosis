using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Diagnosis.App.ViewModels
{
    public interface IHierarchical<T> where T : class
    {
        bool IsRoot { get; }

        bool IsTerminal { get; }

        T Parent { get; }

        ObservableCollection<T> Children { get; }

        ObservableCollection<T> TerminalChildren { get; }

        ObservableCollection<T> NonTerminalChildren { get; }

        IEnumerable<T> AllChildren { get; }

        T Add(T item);

        T AddIfNotExists(T item, bool lookupAllChildren);

        T Remove(T item);

        void Delete();
    }
}