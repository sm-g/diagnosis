namespace Diagnosis.ViewModels
{
    public interface ICheckableHierarchical<T> : ICheckable, IHierarchical<T> where T : class
    {
        int CheckedChildren { get; }
    }
}