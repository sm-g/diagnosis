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

    public interface ICritKeeper
    {
        ICrit Crit { get; }
    }

    public interface IExistTestable
    {
        bool WasEdited { get; }
        bool HasExistingValue { get; set; }
        string[] TestExistingFor { get; }
    }
}