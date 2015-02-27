using Diagnosis.Common.Presentation.DebugTools;
using Diagnosis.Common.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace Diagnosis.Common.Presentation
{
    public class Startuper
    {
        public static void CheckSingleInstance(Guid appGuid, Application app, string appName)
        {
            bool aIsNewInstance = false;
            var myMutex = new Mutex(true, appGuid.ToString(), out aIsNewInstance);
            if (!aIsNewInstance)
            {
                MessageBox.Show("Приложение уже запущено.", appName, MessageBoxButton.OK, MessageBoxImage.Information);
                app.Shutdown();

                return;
            }

        }

        public static void SetWpfCulture()
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(
                CultureInfo.CurrentCulture.IetfLanguageTag)));

            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Error;
        }

      
    }
}
