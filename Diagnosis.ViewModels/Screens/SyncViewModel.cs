using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Sync;
using Diagnosis.ViewModels.Framework;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Diagnosis.ViewModels.Screens
{
    public class SyncViewModel : ScreenBaseViewModel
    {
        private static readonly string serverConStrName = "server";

        private static string _log;

        private string _remoteConStr;
        private string _remoteProvider;
        private string _localConStr;
        private string _localProvider;
        private Syncer syncer;

        public SyncViewModel()
        {
            Title = "Синхронизация";

            LocalConnectionString = NHibernateHelper.ConnectionString;
            // TODO get LocalProviderName 
            LocalProviderName = LocalConnectionString.Contains(".sdf") ? Syncer.SqlCeProvider : Syncer.SqlServerProvider;

            Syncer.MessagePosted += syncer_MessagePosted;
            Syncer.SyncEnded += syncer_SyncEnded;

            var server = ConfigurationManager.ConnectionStrings[serverConStrName];
            if (server != null)
            {
                RemoteConnectionString = server.ConnectionString;
                RemoteProviderName = server.ProviderName;
            }
#if DEBUG
        //    RemoteConnectionString = "Data Source=remote.sdf";
            RemoteProviderName = Syncer.SqlCeProvider;
#endif
        }

        /// <summary>
        /// Remote, server or middle on Client.exe, middle on Server.exe
        /// </summary>
        public string RemoteConnectionString
        {
            get
            {
                return _remoteConStr;
            }
            set
            {
                if (_remoteConStr != value)
                {
                    if (!value.StartsWith("Data Source="))
                    {
                        value = "Data Source=" + value;
                    }

                    _remoteConStr = value;
                    OnPropertyChanged(() => RemoteConnectionString);
                }
            }
        }
        public string LocalConnectionString
        {
            get
            {
                return _localConStr;
            }
            set
            {
                if (_localConStr != value)
                {
                    _localConStr = value;
                    OnPropertyChanged(() => LocalConnectionString);
                }
            }
        }

        public string RemoteProviderName
        {
            get
            {
                return _remoteProvider;
            }
            set
            {
                if (_remoteProvider != value)
                {
                    _remoteProvider = value;
                    OnPropertyChanged(() => RemoteProviderName);
                }
            }
        }
        public string LocalProviderName
        {
            get
            {
                return _localProvider;
            }
            set
            {
                if (_localProvider != value)
                {
                    _localProvider = value;
                    OnPropertyChanged(() => LocalProviderName);
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
                        RemoteConnectionString = result.FileName;
                        RemoteProviderName = Syncer.SqlCeProvider;
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
                    syncer = new Syncer(
                        serverConStr: RemoteConnectionString,
                        clientConStr: LocalConnectionString,
                        serverProviderName: RemoteProviderName);

                    DoWithCursor(syncer.SendFrom(Side.Server), Cursors.AppStarting);
                },
                () => CanSync(true, true));
            }
        }
        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var result = new FileDialogService().ShowSaveFileDialog(null,
                         FileType.Sdf.ToEnumerable(),
                         FileType.Sdf,
                         "diagnosis-ref");

                    if (result.IsValid)
                    {
                        var sdfPath = result.FileName;
                        var remoteConstr = "Data Source=" + sdfPath;

                        syncer = new Syncer(
                            serverConStr: LocalConnectionString,
                            clientConStr: remoteConstr,
                            serverProviderName: LocalProviderName);

                        IEnumerable<Scope> scopes;
                        if (InServerApp())
                            scopes = Scope.Reference.ToEnumerable();
                        else
                            scopes = Scopes.GetOrderedUploadScopes();

                        DoWithCursor(
                            syncer.SendFrom(Side.Server, scopes, true).ContinueWith((t) =>
                            Syncer.Deprovision(remoteConstr, Syncer.SqlCeProvider, scopes)), Cursors.AppStarting);
                    }
                },
                () => CanSync(true, false));
            }
        }

        public RelayCommand UploadCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Syncer syncer;
                    if (InServerApp())
                    {
                        syncer = new Syncer(
                            serverConStr: LocalConnectionString,
                            clientConStr: RemoteConnectionString,
                            serverProviderName: LocalProviderName);
                    }
                    else
                    {
                        syncer = new Syncer(
                            serverConStr: RemoteConnectionString,
                            clientConStr: LocalConnectionString,
                            serverProviderName: RemoteProviderName);
                    }
                    DoWithCursor(syncer.SendFrom(Side.Client), Cursors.AppStarting);
                },
                () => CanSync(true, true));
            }
        }

        public RelayCommand DeprovisRemoteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    DoWithCursor(Syncer.Deprovision(RemoteConnectionString, RemoteProviderName), Cursors.AppStarting);
                },
                () => CanSync(false, true));
            }
        }
        public RelayCommand DeprovisLocalCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    DoWithCursor(Syncer.Deprovision(LocalConnectionString, LocalProviderName), Cursors.AppStarting);
                },
                () => CanSync(true, false));
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

        public bool CanSync(bool local, bool remote)
        {
            bool result = !Syncer.InSync;
            if (local)
            {
                result &= !LocalConnectionString.IsNullOrEmpty() &&
                        (LocalProviderName == Syncer.SqlCeProvider ||
                         LocalProviderName == Syncer.SqlServerProvider);
            }
            if (remote)
            {
                result &= !RemoteConnectionString.IsNullOrEmpty() &&
                        (RemoteProviderName == Syncer.SqlCeProvider ||
                         RemoteProviderName == Syncer.SqlServerProvider);

            }
            return result;
        }

        bool InServerApp()
        {
            return AppDomain.CurrentDomain.FriendlyName.Contains("Server");
        }

        private void syncer_MessagePosted(object sender, StringEventArgs e)
        {
            Log += string.Format("[{0:mm:ss:fff}] {1}\n", DateTime.Now, e.str);
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
                var settings = new ConnectionStringSettings(serverConStrName, RemoteConnectionString, RemoteProviderName);
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