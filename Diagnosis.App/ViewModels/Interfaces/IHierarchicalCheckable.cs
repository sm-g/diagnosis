using System;
namespace Diagnosis.App.ViewModels
{
    public interface IHierarchicalCheckable : ICheckable
    {
        int CheckedChildren { get; }
    }
}
