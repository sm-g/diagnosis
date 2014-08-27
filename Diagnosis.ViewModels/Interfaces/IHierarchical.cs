using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace Diagnosis.ViewModels
{
    public interface IHierarchical<T> where T : class
    {
        event HierarchicalEventHandler<T> ChildrenChanged;
        event HierarchicalEventHandler<T> ParentChanged;

        bool IsRoot { get; }
        bool IsTerminal { get; }
        bool IsParent { get; }
        bool IsExpanded { get; set; }

        T Parent { get; }

        ObservableCollection<T> Children { get; }

        ObservableCollection<T> TerminalChildren { get; }

        ObservableCollection<T> NonTerminalChildren { get; }

        IEnumerable<T> AllChildren { get; }

        T Add(T item);

        T Add(IEnumerable<T> items);

        T AddIfNotExists(T item);
        T AddIfNotExists(T item, Func<T, T, bool> equalsComparator);

        T Remove(T item);

        T Remove(IEnumerable<T> items);

        void Remove();
    }
}