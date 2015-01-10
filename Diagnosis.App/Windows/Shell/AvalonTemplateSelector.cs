using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Diagnosis.Models;
using System.Diagnostics;
using Xceed.Wpf.AvalonDock.Layout;
using Diagnosis.ViewModels.Screens;

namespace Diagnosis.App.Windows.Shell
{
    public class AvalonTemplateSelector : DataTemplateSelector
    {
        Dictionary<Type, DataTemplate> @sw;

        public AvalonTemplateSelector()
        {
            @sw = new Dictionary<Type, DataTemplate> { 
                { typeof(ToolViewModel), PanelTemplate},
                { typeof(ScreenBase), ScreenTemplate}                                                
            };
        }

        public DataTemplate PanelTemplate { get; set; }
        public DataTemplate ScreenTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var lc = item as LayoutContent;
            if (lc != null)
            {
                if (lc.Content is ToolViewModel) return PanelTemplate;
                if (lc.Content is ScreenBase) return ScreenTemplate;
            }

            return base.SelectTemplate(item, container);
        }

    }
}