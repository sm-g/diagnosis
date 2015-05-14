using Diagnosis.Models;
using Diagnosis.ViewModels.Controls;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    internal interface IFilterableList
    {
        IFilter Filter { get; }
    }

    public interface IKeeper<T>
    {
        T Kept { get; }
    }
    public interface IHolderKeeper //: IKeeper<IHrsHolder>
    {
        IHrsHolder Holder { get; }
    }

    public interface ICritKeeper //: IKeeper<ICrit>
    {
        ICrit Crit { get; }
    }

    public interface IExistTestable
    {
        bool WasEdited { get; }
        bool HasExistingValue { get; set; }
        string[] TestExistingFor { get; }
        string ThisValueExistsMessage { get; }
    }
}