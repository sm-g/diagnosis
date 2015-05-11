using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels
{
    public interface IFocusable
    {
        bool IsFocused { get; set; }
    }
}
