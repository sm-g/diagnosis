using System;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace Diagnosis.Common.Presentation
{
    public class Startuper
    {
        static Mutex mutex;

        public static bool CheckSingleInstance(Guid appGuid, Application app, string appName)
        {
            // unique id for global mutex - Global prefix means it is global to the machine
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);
            bool createdNew;

            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            mutex = new Mutex(false, mutexId, out createdNew, securitySettings);
            if (!createdNew)
            {
                MessageBox.Show("Приложение уже запущено.", appName, MessageBoxButton.OK, MessageBoxImage.Information);
                app.Shutdown();

                return false;
            }
            return true;
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