using System;
namespace Diagnosis.ViewModels
{
    public interface ICheckable
    {
        bool IsChecked { get; set; }
        bool IsNonCheckable { get; }
        void ToggleChecked();
    }
}
