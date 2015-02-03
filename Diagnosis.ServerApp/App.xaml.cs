using Diagnosis.Common.DebugTools;
using Diagnosis.Common.Util;
using Diagnosis.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace Diagnosis.ServerApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(App));
        private static bool inExit = false;
        private const string BackupFolder = "Backup\\";
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        protected override void OnExit(ExitEventArgs e)
        {
            inExit = true;
            //this.Send(Event.Shutdown);
            Diagnosis.ServerApp.Properties.Settings.Default.Save();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            bool aIsNewInstance = false;
            var myMutex = new Mutex(true, "2c2ee38e-31c5-45f5-8fde-4a9a126df452", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                MessageBox.Show("Приложение уже запущено.", "Diagnosis Server", MessageBoxButton.OK, MessageBoxImage.Information);
                App.Current.Shutdown();
                return;
            }

            // command line args
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "-inmemory")
                {
                    NHibernateHelper.InMemory = true;
                }
            }

            // wpf culture
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(
                CultureInfo.CurrentCulture.IetfLanguageTag)));
            // enum localization
            LocalizableDescriptionAttribute.ResourcesType = typeof(Diagnosis.ServerApp.Properties.Resources);
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Error;
#if DEBUG
            new DebugOutput(0);

            var debugVm = new LogTraceListener()
            {
                FilterContains = Diagnosis.ServerApp.Properties.Settings.Default.DebugFilter ?? "",
                FilterOn = Diagnosis.ServerApp.Properties.Settings.Default.DebugFilterOn
            };
            debugVm.PropertyChanged += (s, e1) =>
            {
                switch (e1.PropertyName)
                {
                    case "FilterContains":
                        Diagnosis.ServerApp.Properties.Settings.Default.DebugFilter = debugVm.FilterContains;
                        break;
                }
            };
            var debWin = new DebugWindow(debugVm);
            debWin.Closing += (s, e1) =>
            {
                Diagnosis.ServerApp.Properties.Settings.Default.DebugFilterOn = debugVm.FilterOn;

            };
            debWin.Show();

            NHibernateHelper.ShowSql = !NHibernateHelper.InMemory;
#endif

           // DbMaintenance();

            var main = new MainWindow();

            Application.Current.MainWindow = main;
            Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
            main.Show();
        }
        [DebuggerStepThrough]
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.ErrorFormat("Unhandled: {0}", e.ExceptionObject as Exception);
        }

        public class MyLock : log4net.Appender.FileAppender.MinimalLock
        {
            // from http://stackoverflow.com/questions/2533403/log4net-how-to-disable-creation-of-empty-log-file-on-app-start
            public override void ReleaseLock()
            {
                base.ReleaseLock();

                if (inExit)
                {
                    var logFile = new FileInfo(CurrentAppender.File);
                    if (logFile.Exists && logFile.Length <= 0)
                    {
                        logFile.Delete();
                    }
                }
            }
        }
    }
}
