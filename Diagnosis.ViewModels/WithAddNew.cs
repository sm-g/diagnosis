using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels
{
    /// <summary>
    /// Обертка для отличия специального случая среди элементов в коллекции ItemsSource.
    /// </summary>
    public class WithAddNew : ViewModelBase
    {
        public bool IsAddNew { get; private set; }
        public object Content { get; internal set; }

        public WithAddNew(object item)
        {
            Content = item;
        }
        public WithAddNew()
        {
            IsAddNew = true;
        }
    }
}
