using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;

namespace Diagnosis.App.Controls
{
    class TreeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FullTreeWithSearch { get; set; }
        public DataTemplate FullTreeLight { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return null;
        }
    }
}
