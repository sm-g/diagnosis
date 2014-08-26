using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Diagnosis.ViewModels;

namespace Diagnosis.App.Controls.Card
{
    public class AddNewDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate AddNewTemplate { get; set; }

        public override DataTemplate
              SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is WithAddNew)
            {
                if ((item as WithAddNew).IsAddNew)
                {
                    return AddNewTemplate;
                }
                return DefaultTemplate;
            }

            return null;
        }
    }
}
