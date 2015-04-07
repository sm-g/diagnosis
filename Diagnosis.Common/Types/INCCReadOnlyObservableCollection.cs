using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Diagnosis.Common.Types
{
    public class INCCReadOnlyObservableCollection<T> : ReadOnlyObservableCollection<T>
    {
        public INCCReadOnlyObservableCollection(ObservableCollection<T> list)
            : base(list)
        {
        }

        public event NotifyCollectionChangedEventHandler CollectionChangedWrapper
        {
            add { CollectionChanged += value; }
            remove { CollectionChanged -= value; }
        }
    }

}
