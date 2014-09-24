using Diagnosis.App.Windows;
using log4net;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Diagnosis.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly ILog logger = LogManager.GetLogger(typeof(App));

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // command line args
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "-inmemory")
                {
                    Diagnosis.Data.NHibernateHelper.InMemory = true;
                }
            }

            // wpf culture
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(
                CultureInfo.CurrentCulture.IetfLanguageTag)));

            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Error;
            AppDomain.CurrentDomain.UnhandledException += (s, e1) =>
            {
                logger.ErrorFormat("Unhandled: {0}", e1.ExceptionObject as Exception);
            };
#if DEBUG
            new DebugOutput(0);
            new DebugWindow().Show();
#endif

            var main = new MainWindow();
            Application.Current.MainWindow = main;
            Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
            main.Show();
        }
    }
}