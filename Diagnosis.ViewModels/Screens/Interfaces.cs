using Diagnosis.Models;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    internal interface IFilterableList
    {
        //  FilterViewModel<T> Filter { get; }
    }

    public interface IHolderKeeper
    {
        IHrsHolder Holder { get; }
    }
}