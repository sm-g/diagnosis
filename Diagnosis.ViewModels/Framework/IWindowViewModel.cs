using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public interface IWindowViewModel
    {
        string Title { get; set; }

        bool IsClosed { get; set; }

        bool IsActive { get; set; }
    }
}
