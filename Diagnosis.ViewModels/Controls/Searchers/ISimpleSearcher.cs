using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Controls
{
    public interface ISimpleSearcher<T>
    {
        IEnumerable<T> Search(string query);
    }

    public interface IHierarchicalSearcher<T> : ISimpleSearcher<T>
    {
        bool WithNonCheckable { get; }
    }
}