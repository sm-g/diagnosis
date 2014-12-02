using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace Diagnosis.ViewModels
{
    public interface IHierarchical<T> where T : class
    {
        event EventHandler<HierarchicalEventAgrs<T>> ChildrenChanged;
        event EventHandler<HierarchicalEventAgrs<T>> ParentChanged;

        bool IsRoot { get; }
        bool IsTerminal { get; }
        bool IsExpanded { get; set; }

        T Parent { get; }

        ObservableCollection<T> Children { get; }

        IEnumerable<T> AllChildren { get; }

        T Add(T item);

        T Add(IEnumerable<T> items);

        T Remove(T item);

        T Remove(IEnumerable<T> items);

        void Remove();

        void ExpandParents();
    }
}