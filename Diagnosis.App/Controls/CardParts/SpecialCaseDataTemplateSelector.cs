﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Diagnosis.ViewModels;

namespace Diagnosis.App.Controls.CardParts
{
    public class SpecialCaseDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate AddNewTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is SpecialCaseItem)
            {
                if ((item as SpecialCaseItem).IsAddNew)
                {
                    return AddNewTemplate;
                }
                return DefaultTemplate;
            }

            return null;
        }
    }

}