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

    public interface IHolderKeeper
    {
        IHrsHolder Holder { get; }
    }
}