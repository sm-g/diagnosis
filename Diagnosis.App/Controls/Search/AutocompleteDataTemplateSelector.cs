using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;

namespace Diagnosis.App.Controls.Search
{
    public class AutocompleteDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DiagnosisTemplate { get; set; }
        public DataTemplate WordTemplate { get; set; }

        public override DataTemplate
              SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                if (item is DiagnosisViewModel)
                {
                    return DiagnosisTemplate;
                }
                else if (item is WordViewModel)
                {
                    return WordTemplate;
                }
            }

            return null;
        }
    }
}
