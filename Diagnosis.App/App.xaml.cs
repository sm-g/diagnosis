using Diagnosis.App.Windows;
using log4net;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using EventAggregator;
using Diagnosis.Common;
using System.Diagnostics;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Diagnosis.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(App));

        public App()
        {
            Exit += (s, e) =>
            {
                this.Send(Events.Shutdown);
                Diagnosis.App.Properties.Settings.Default.Save();
            };

            Startup += (s, e) =>
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
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#if DEBUG
                new DebugOutput(0);
                new DebugWindow().Show();
#endif

                var main = new MainWindow();
                Application.Current.MainWindow = main;
                Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
                main.Show();
            };
        }
        [DebuggerStepThrough]

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.ErrorFormat("Unhandled: {0}", e.ExceptionObject as Exception);

        }
    }
}