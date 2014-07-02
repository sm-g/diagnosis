using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace Diagnosis.App.ViewModels
{
    public interface IHierarchical<T> where T : class
    {
        event HierarchicalEventHandler<T> ChildrenChanged;

        bool IsRoot { get; }

        bool IsTerminal { get; }

        T Parent { get; }

        ObservableCollection<T> Children { get; }

        ObservableCollection<T> TerminalChildren { get; }

        ObservableCollection<T> NonTerminalChildren { get; }

        IEnumerable<T> AllChildren { get; }

        T Add(T item);

        T Add(IEnumerable<T> items);

        T AddIfNotExists(T item, bool lookupAllChildren);

        T Remove(T item);

        T Remove(IEnumerable<T> items);

        void Remove();
    }
}