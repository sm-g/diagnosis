using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Diagnosis.App.ViewModels;

namespace Diagnosis.App.Controls
{
    public class AutocompleteDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DiagnosisTemplate { get; set; }
        public DataTemplate SymptomTemplate { get; set; }

        public override DataTemplate
              SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                if (item is DiagnosisViewModel)
                {
                    return DiagnosisTemplate;
                }
                if (item is SymptomViewModel)
                {
                    return SymptomTemplate;
                }
            }

            return null;
        }
    }
}
