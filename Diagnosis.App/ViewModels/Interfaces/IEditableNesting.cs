using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.App.ViewModels
{
    interface IEditableNesting
    {
        Editable Editable { get; }
        bool IsEmpty { get; }
    }
}
