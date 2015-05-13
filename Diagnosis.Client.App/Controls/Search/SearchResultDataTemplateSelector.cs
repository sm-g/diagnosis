using Diagnosis.ViewModels.Screens;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.Client.App.Controls.Search
{
    public class SearchResultDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CritTemplate { get; set; }

        public DataTemplate DefaultTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is HrsSearchResultViewModel)
                return DefaultTemplate;
            if (item is CritSearchResultViewModel)
                return CritTemplate;
            return null;
        }
    }
}