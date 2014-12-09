using Diagnosis.App.Windows;
using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Versions;
using log4net;
using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
                        NHibernateHelper.InMemory = true;
                    }
                }

                // wpf culture
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(
                    CultureInfo.CurrentCulture.IetfLanguageTag)));

                System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Error;
#if DEBUG
                new DebugOutput(0);
                new DebugWindow().Show();
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
#if !DEBUG
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
            FileHelper.Backup(sdfPath, BackupFolder, 5, 7);

            // migrate to last version
            var a = "Diagnosis.Data.dll";
            var db = "sqlserverce";
            var task = "";//"-t rollback";
            // Process.Start(Migrate.exe, string.Format("-c \"{0}\" -db {1} -a {2} -o -of Backup\\migrated-{3:yyyy-MM-dd-HH-mm-ss}.sql {4}", constr, db, a, DateTime.UtcNow, task));
            var rollback = false;
            if (rollback)
            {
                new Migrator(constr, BackupFolder).Rollback();
            }
            else
            {
                new Migrator(constr, BackupFolder).MigrateToLatest();
            }
#endif
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