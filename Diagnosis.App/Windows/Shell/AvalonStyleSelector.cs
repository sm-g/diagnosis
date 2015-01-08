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
    public class AvalonStyleSelector : StyleSelector
    {
        public Style PanelStyle { get; set; }
        public Style ScreenStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ToolViewModel)
            {
                return PanelStyle;
            }

            if (item is ScreenBase)
            {
                return ScreenStyle;
            }

            return base.SelectStyle(item, container);
        }

    }
}