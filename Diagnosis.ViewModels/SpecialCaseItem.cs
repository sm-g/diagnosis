using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels
{
    /// <summary>
    /// Обертка для отличия специального случая среди элементов в коллекции ItemsSource.
    /// </summary>
    public class SpecialCaseItem
    {
        public enum Cases
        {
            AddNew
        }

        public bool IsAddNew { get; private set; }
        public ViewModelBase Content { get; private set; }

        public SpecialCaseItem(ViewModelBase item)
        {
            Content = item;
        }
        public SpecialCaseItem(Cases @case)
        {
            switch (@case)
            {
                case Cases.AddNew:
                    IsAddNew = true;
                    break;
                default:
                    break;
            }
        }
    }


    public static class SpecialCaseItemExtensions
    {
        public static T To<T>(this SpecialCaseItem sci) where T : class
        {
            if (sci.Content is T)
                return sci.Content as T;

            throw new ArgumentException("Wrong type argument. SpecialCaseItem.Content is " + sci.Content.GetType().ToString());
        }
    }
}
