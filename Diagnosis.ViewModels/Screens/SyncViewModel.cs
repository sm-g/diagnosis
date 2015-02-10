using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Sync;
using Diagnosis.ViewModels.Framework;
using EventAggregator;
using System;

using System;

using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Diagnosis.ViewModels.Screens
{
    public class SyncViewModel : ScreenBaseViewModel
    {
        private static readonly string serverConStrName = "server";
        private const string sqlCeProvider = "System.Data.SqlServerCE.4.0";
        private const string sqlServerProvider = "System.Data.SqlClient";

        private static string _log;

        private string _conStr;
        private string _provider;
        private string clientConStr;
        private Syncer syncer;

        public SyncViewModel()
        {
            Title = "Синхронизация";

            clientConStr = NHibernateHelper.ConnectionString;
            Syncer.MessagePosted += syncer_MessagePosted;
            Syncer.SyncEnded += syncer_SyncEnded;

            var server = ConfigurationManager.ConnectionStrings[serverConStrName];
            if (server != null)
            {
                ConnectionString = server.ConnectionString;
                ProviderName = server.ProviderName;
            }
#if DEBUG
            ConnectionString = "Data Source=diagnosis2.sdf";
            ProviderName = sqlCeProvider;
#endif
        }

        public string ConnectionString
        {
            get
            {
                return _conStr;
            }
            set
            {
                if (_conStr != value)
                {
                    if (!value.StartsWith("Data Source="))
                    {
                        value = "Data Source=" + value;
                    }

                    _conStr = value;
                    OnPropertyChanged(() => ConnectionString);
                }
            }
        }

        public string ProviderName
        {
            get
            {
                return _provider;
            }
            set
            {
                if (_provider != value)
                {
                    _provider = value;
                    OnPropertyChanged(() => ProviderName);
                }
            }
        }

        public RelayCommand OpenSdfCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var result = new FileDialogService().ShowOpenFileDialog(null,
                         FileType.Sdf.ToEnumerable(),
                         FileType.Sdf,
                         "diagnosis");

                    if (result.IsValid)
                    {
                        ConnectionString = result.FileName;
                        ProviderName = sqlCeProvider;
                    }
                });
            }
        }

        public RelayCommand DownloadCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    syncer = new Syncer(ConnectionString, clientConStr, ProviderName);
                    DoWithCursor(syncer.SendFrom(Db.Server), Cursors.AppStarting);
                },
                () => CanSync);
            }
        }

        public RelayCommand UploadCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    syncer = new Syncer(ConnectionString, clientConStr, ProviderName);
                    DoWithCursor(syncer.SendFrom(Db.Client), Cursors.AppStarting);
                },
                () => CanSync);
            }
        }

        public RelayCommand DeprovisClientCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    syncer = new Syncer(ConnectionString, clientConStr, ProviderName);
                    DoWithCursor(syncer.Deprovision(Db.Client), Cursors.AppStarting);
                },
                () => CanSync);
            }
        }

        public RelayCommand DeprovisServerCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    syncer = new Syncer(ConnectionString, clientConStr, ProviderName);
                    DoWithCursor(syncer.Deprovision(Db.Server), Cursors.AppStarting);
                },
                () => CanSync);
            }
        }

        public string Log
        {
            get
            {
                return _log;
            }
            set
            {
                if (_log != value)
                {
                    _log = value;
                    OnPropertyChanged(() => Log);
                }
            }
        }

        public bool CanSync
        {
            get
            {
                return !Syncer.InSync && !ConnectionString.IsNullOrEmpty() &&
                (ProviderName == sqlCeProvider ||
                 ProviderName == sqlServerProvider);
            }
        }

        private void syncer_MessagePosted(object sender, StringEventArgs e)
        {
            Log += e.str + '\n';
        }

        private void syncer_SyncEnded(object sender, TimeSpanEventArgs e)
        {
            Log += "\n=== " + e.ts.ToString() + "\n";
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                CommandManager.InvalidateRequerySuggested()));
        }

        private void DoWithCursor(Task act, Cursor cursor)
        {
            Mouse.OverrideCursor = cursor;
            act.ContinueWith((t) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                    Mouse.OverrideCursor = null));
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Syncer.SyncEnded -= syncer_SyncEnded;
                Syncer.MessagePosted -= syncer_MessagePosted;

                // http://stackoverflow.com/questions/502411/change-connection-string-reload-app-config-at-run-time
                // clear all connection strings and save new
                var settings = new ConnectionStringSettings(serverConStrName, ConnectionString, ProviderName);
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
                connectionStringsSection.ConnectionStrings.Clear();
                connectionStringsSection.ConnectionStrings.Add(settings);
                config.Save();
                ConfigurationManager.RefreshSection("connectionStrings");
            }
            base.Dispose(disposing);
        }
    }
}