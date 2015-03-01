﻿using Diagnosis.Client.App.Themes;
using Diagnosis.Client.App.Windows.Shell;
using Diagnosis.Common;
using Diagnosis.Common.Presentation;
using Diagnosis.Common.Presentation.DebugTools;
using Diagnosis.Common.Util;
using Diagnosis.Data;
using Diagnosis.Data.Versions;
using log4net;
using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Diagnosis.Client.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(App));
        private static bool inExit = false;
        private const string appGuid = "ac2ee38e-31c5-45f5-8fde-4a9a126df451";
        private SplashScreen splash = null;

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            inExit = true;
            this.Send(Event.Shutdown);
            Diagnosis.Client.App.Properties.Settings.Default.Save();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Startuper.CheckSingleInstance(new Guid(appGuid), App.Current, "Diagnosis");

            // command line args
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "-inmemory")
                {
                    NHibernateHelper.InMemory = true;
                }
            }

#if !DEBUG
            splash = new SplashScreen(@"Resources\Images\splash.png");
            splash.Show(false);
#endif
            Startuper.SetWpfCulture();

            // enum localization
            LocalizableDescriptionAttribute.ResourcesType = typeof(Diagnosis.Client.App.Properties.Resources);

            // themes
            MyThemeManager.Initialize();

#if DEBUG
            StartDebugTools();
#endif

            DbMaintenance();

            StartMainWindow();
        }

        private void StartMainWindow()
        {
            var main = new MainWindow();

            if (splash != null)
                main.Loaded += (s, e1) =>
                {
                    splash.Close(TimeSpan.FromMilliseconds(100));
                    main.Focus();
                };

            Application.Current.MainWindow = main;
            Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
            main.Show();
        }

        private static void DbMaintenance()
        {
            var con = ConfigurationManager.ConnectionStrings[Constants.clientConStrName];
            NHibernateHelper.Init(con);

            if (NHibernateHelper.InMemory)
                return;

            // create db
            SqlCeHelper.CreateSqlCeByConStr(NHibernateHelper.ConnectionString);

            // backup
#if !DEBUG
            var sdfPath = new SqlCeConnectionStringBuilder(NHibernateHelper.ConnectionString).DataSource;
            FileHelper.Backup(sdfPath, Constants.BackupDir, 5, 7);
#endif

            bool? migrateUp = null;

            // migrate to last version in release
#if !DEBUG
            migrateUp = true;
#endif
            if (migrateUp.HasValue)
            {
                if (migrateUp.Value)
                {
                    new Migrator(NHibernateHelper.ConnectionString, Constants.BackupDir).MigrateToLatest();
                }
                else
                {
                    new Migrator(NHibernateHelper.ConnectionString, Constants.BackupDir).Rollback();
                }
            }
        }

        private static void StartDebugTools()
        {
            new DebugOutput(0);

            var debugVm = new LogTraceListener()
            {
                FilterContains = Diagnosis.Client.App.Properties.Settings.Default.DebugFilter ?? "",
                FilterOn = Diagnosis.Client.App.Properties.Settings.Default.DebugFilterOn
            };
            debugVm.PropertyChanged += (s, e1) =>
            {
                switch (e1.PropertyName)
                {
                    case "FilterContains":
                        Diagnosis.Client.App.Properties.Settings.Default.DebugFilter = debugVm.FilterContains;
                        break;
                }
            };
            var debWin = new DebugWindow(debugVm);
            debWin.Closing += (s, e1) =>
            {
                Diagnosis.Client.App.Properties.Settings.Default.DebugFilterOn = debugVm.FilterOn;
            };
            debWin.Show();

            // NHibernateHelper.ShowSql = !NHibernateHelper.InMemory;
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