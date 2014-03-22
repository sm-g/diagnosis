using System;
namespace Diagnosis.App.ViewModels
{
    public interface IHierarchicalCheckable
    {
        int CheckedChildren { get; }
    }
}
