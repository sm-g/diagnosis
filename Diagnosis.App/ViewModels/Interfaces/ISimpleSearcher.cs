using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public interface ISimpleSearcher<T>
    {
        IEnumerable<T> Collection { get; }
        IEnumerable<T> Search(string query);

        bool WithNonCheckable { get; }
        bool WithChecked { get; }
        bool WithCreatingNew { get; }
        bool AllChildren { get; }
    }
}