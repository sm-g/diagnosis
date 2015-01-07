using Diagnosis.App.Windows;
using Diagnosis.Common;
using Diagnosis.Common.Util;
using Diagnosis.Data;
using Diagnosis.Data.Versions;
using log4net;
using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
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
        private static readonly ILog logger = LogManager.GetLogger(typeof(App));
        private static bool inExit = false;
        private const string BackupFolder = "Backup\\";

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Exit += (s, e) =>
            {
                inExit = true;
                this.Send(Event.Shutdown);
                Diagnosis.App.Properties.Settings.Default.Save();
            };

            Startup += (s, e) =>
            {
                bool aIsNewInstance = false;
                var myMutex = new Mutex(true, "ac2ee38e-31c5-45f5-8fde-4a9a126df451", out aIsNewInstance);
                if (!aIsNewInstance)
                {
                    MessageBox.Show("Приложение уже запущено.", "Diagnosis", MessageBoxButton.OK, MessageBoxImage.Information);
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
                LocalizableDescriptionAttribute.ResourcesType = typeof(Diagnosis.App.Properties.Resources);

                System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Error;
#if DEBUG
                new DebugOutput(0);
                new DebugWindow().Show();
                NHibernateHelper.ShowSql = !NHibernateHelper.InMemory;

#endif
                DbMaintenance();

                var main = new MainWindow();
                Application.Current.MainWindow = main;
                Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
                main.Show();
            };
        }

        [DebuggerStepThrough]
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.ErrorFormat("Unhandled: {0}", e.ExceptionObject as Exception);
        }

        private static void DbMaintenance()
        {
            if (NHibernateHelper.InMemory)
                return;

            // create db
            var constr = NHibernateHelper.Configuration.GetProperty(NHibernate.Cfg.Environment.ConnectionString);
            var builder = new SqlCeConnectionStringBuilder(constr);
            var sdfPath = builder.DataSource;
            if (!System.IO.File.Exists(sdfPath))
            {
                using (var engine = new SqlCeEngine(constr))
                {
                    engine.CreateDatabase();
                }
            }

            // backup
#if !DEBUG
            FileHelper.Backup(sdfPath, BackupFolder, 5, 7);
#endif

            bool? migrateUp = null;

            // migrate to last version in release
#if !DEBUG
            migrateUp = true;
#endif
            if (migrateUp.HasValue)
                if (migrateUp.Value)
                {
                    new Migrator(constr, BackupFolder).MigrateToLatest();
                }
                else
                {
                    new Migrator(constr, BackupFolder).Rollback();
                }
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