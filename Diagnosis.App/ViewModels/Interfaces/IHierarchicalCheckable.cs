using System;
namespace Diagnosis.App.ViewModels
{
    public interface IHierarchicalCheckable<T> : ICheckable, IHierarchical<T> where T : class
    {
        int CheckedChildren { get; }
        bool IsFiltered { get; set; }

    }
}
