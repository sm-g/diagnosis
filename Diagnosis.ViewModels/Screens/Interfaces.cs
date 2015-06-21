using Diagnosis.Models;
using Diagnosis.ViewModels.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    internal interface IFilterableList
    {
        IFilter Filter { get; }
        IEnumerable<CheckableBase> Items { get; }
    }

    public interface IKeeper<T>
    {
        T Kept { get; }
    }
    /// <summary>
    /// ViewModel c IHrsHolder
    /// </summary>
    public interface IHrsHolderKeeper //: IKeeper<IHrsHolder>
    {
        IHrsHolder Holder { get; }
    }

    /// <summary>
    /// ViewModel c ICrit
    /// </summary>
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