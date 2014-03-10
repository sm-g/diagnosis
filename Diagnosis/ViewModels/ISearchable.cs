using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Text;

namespace Diagnosis.ViewModels
{
    public interface ISearchable
    {
        string Representation { get; }
        ICommand SearchCommand { get; }
        bool IsSearchActive { get; set; }
        bool IsSearchFocused { get; set; }
    }
}
