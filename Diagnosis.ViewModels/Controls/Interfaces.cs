using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Controls
{
    internal interface IFilter
    {
        event EventHandler Cleared;
        event EventHandler Filtered;

        string Query { get; set; }
        bool IsQueryEmpty { get; }
        ICommand ClearCommand { get; }
        ICommand FilterCommand { get; }
        int AutoFilterMinQueryLength { get; set; }
        bool DoAutoFilter { get; set; }
        bool AutoFiltered { get; set; }

        void Clear();
        void Filter();
    }
}
