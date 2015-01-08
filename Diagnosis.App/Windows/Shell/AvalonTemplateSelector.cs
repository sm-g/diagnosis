using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Diagnosis.Models;
using System.Diagnostics;
using Diagnosis.ViewModels.Screens;

namespace Diagnosis.App.Windows.Shell
{
    public class AvalonTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PanelTemplate { get; set; }
        public DataTemplate ScreenTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ToolViewModel)
            {
                return PanelTemplate;
            }

            if (item is ScreenBase)
            {
                return ScreenTemplate;
            }

            return base.SelectTemplate(item, container);
        }

    }
}