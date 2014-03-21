using System;
namespace Diagnosis.ViewModels
{
    public interface IHierarchicalCheckable
    {
        int CheckedChildren { get; }
    }
}
