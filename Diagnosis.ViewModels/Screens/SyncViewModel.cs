using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Diagnosis.ViewModels.Controls;
using Diagnosis.ViewModels.Framework;
using EventAggregator;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class SyncViewModel : ScreenBaseViewModel
    {
        private static string _log;

        private string _localConStr;
        private string _localProvider;
        private DataConnectionViewModel _remote;

        public SyncViewModel(ConnectionInfo server = new ConnectionInfo())
        {
            Title = "Синхронизация";

            LocalConnectionString = NHibernateHelper.Default.ConnectionString;
            LocalProviderName = LocalConnectionString.Contains(".sdf") ? Constants.SqlCeProvider : Constants.SqlServerProvider;

            Remote = new DataConnectionViewModel(server);

            Syncer.MessagePosted += syncer_MessagePosted;
            Syncer.SyncEnded += syncer_SyncEnded;
        }

        public DataConnectionViewModel Remote
        {
            get
            {
                return _remote;
            }
            set
            {
                if (_remote != value)
                {
                    _remote = value;
                    OnPropertyChanged(() => Remote);
                }
            }
        }

        /// <summary>
        /// Remote, server or middle on Client.exe, middle on Server.exe
        /// </summary>
        public string RemoteConnectionString
        {
            get { return Remote.ConnectionString; }
        }

        public string RemoteProviderName
        {
            get { return Remote.ProviderName; }
        }

        /// <summary>
        /// Local, NHibernate connected.
        /// </summary>
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

        /// <summary>
        /// Загружает справочные данные с удаленной серверной БД на клиент.
        /// </summary>
        public RelayCommand DownloadCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    Contract.Requires(Constants.IsClient);

                    var syncer = new Syncer(
                         serverConStr: RemoteConnectionString,
                         clientConStr: LocalConnectionString,
                         serverProviderName: RemoteProviderName);

                    Mouse.OverrideCursor = Cursors.AppStarting;

                    var installedVocs = Session.Query<Vocabulary>();
                    await syncer
                        .WithoutDoctors()
                        .WithInstalledVocs(installedVocs)
                        .SendFrom(Side.Server);

                    // после загрузки проверяем справочные сущности на совпадение
                    var checker = new AfterSyncChecker(Session);
                    checker.Replaced += (s, e) =>
                    {
                        if (e.list.Count() > 0)
                            Log += string.Format("[{0:mm:ss:fff}] replaced {1} {2}\n", DateTime.Now, e.list.Count(), e.list.First().Actual.GetType().Name);
                    };
                    var scopesToDeprovision = checker.CheckReferenceEntitiesAfterDownload(syncer.AddedOnServerIdsPerType);

                    // deprovision scopes обновленных сущностей
                    await Syncer.Deprovision(LocalConnectionString, LocalProviderName, scopesToDeprovision);
                    Log += "\n";

                    new VocLoader(Session).AfterSyncVocs(syncer.DeletedOnServerIdsPerType);

                    Mouse.OverrideCursor = null;
                },
                () => CanSync(true, true));
            }
        }

        /// <summary>
        /// Выгружает области из локальной в промежуточную БД.
        /// С сервера - справочные, с клиента - остальные.
        ///
        /// Промежуточная БД готова к последующей синхронизации.
        /// </summary>
        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    var result = new FileDialogService().ShowSaveFileDialog(null,
                         FileType.Sdf.ToEnumerable(),
                         FileType.Sdf,
                         "diagnosis-exchange");

                    if (result.IsValid)
                    {
                        var sdfPath = result.FileName;
                        var sdfFileConstr = "Data Source=" + sdfPath;

                        var syncer = new Syncer(
                             serverConStr: LocalConnectionString,
                             clientConStr: sdfFileConstr,
                             serverProviderName: LocalProviderName);

                        IEnumerable<Scope> scopes;
                        if (Constants.IsClient)
                            scopes = Scopes.GetOrderedUploadScopes();
                        else
                            scopes = Scopes.GetOrderedDownloadScopes();

                        // создаем промежуточную БД
                        SqlHelper.CreateSqlCeByConStr(sdfFileConstr);

                        Mouse.OverrideCursor = Cursors.AppStarting;

                        // выгрузка в существующую БД, которая может быть изменена после этого
                        // можно создавать файл заново
                        // или сначала депровизить, чтобы добавить данные к существующим
                        await Syncer.Deprovision(sdfFileConstr, Constants.SqlCeProvider, scopes);

                        if (Constants.IsClient)
                            await syncer.WithoutCustomVocsInDoc().SendFrom(Side.Server, scopes);
                        else
                            await syncer.WithoutDoctors().SendFrom(Side.Server, scopes);

                        // после выгрузки сразу готовим промежуточную БД к следующей синхронизации
                        await Syncer.Deprovision(sdfFileConstr, Constants.SqlCeProvider, scopes);

                        Mouse.OverrideCursor = null;
                    }
                },
                () => CanSync(true, false));
            }
        }

        /// <summary>
        /// Загружает клиентские данные с клиента на сервер.
        /// </summary>
        public RelayCommand UploadCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Syncer syncer;
                    if (Constants.IsClient)
                    {
                        syncer = new Syncer(
                            serverConStr: RemoteConnectionString,
                            clientConStr: LocalConnectionString,
                            serverProviderName: RemoteProviderName);
                    }
                    else
                    {
                        syncer = new Syncer(
                            serverConStr: LocalConnectionString,
                            clientConStr: RemoteConnectionString,
                            serverProviderName: LocalProviderName);
                    }
                    DoWithCursor(syncer.WithoutCustomVocsInDoc().SendFrom(Side.Client), Cursors.AppStarting);
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
                        (LocalProviderName == Constants.SqlCeProvider ||
                         LocalProviderName == Constants.SqlServerProvider);
            }
            if (remote)
            {
                result &= !RemoteConnectionString.IsNullOrEmpty() &&
                        (RemoteProviderName == Constants.SqlCeProvider ||
                         RemoteProviderName == Constants.SqlServerProvider);
            }
            return result;
        }

        private void syncer_MessagePosted(object sender, StringEventArgs e)
        {
            Log += string.Format("[{0:mm:ss:fff}] {1}\n", DateTime.Now, e.str);
        }

        private void syncer_SyncEnded(object sender, TimeSpanEventArgs e)
        {
            Log += "\n=== " + e.ts.ToString() + "\n";

            uiTaskFactory.StartNew(
                CommandManager.InvalidateRequerySuggested);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Syncer.SyncEnded -= syncer_SyncEnded;
                Syncer.MessagePosted -= syncer_MessagePosted;

                Remote.Dispose();

                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerConstrSettingName, RemoteConnectionString }.AsParams(MessageKeys.Name, MessageKeys.Value));
                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerProviderSettingName, RemoteProviderName }.AsParams(MessageKeys.Name, MessageKeys.Value));
            }
            base.Dispose(disposing);
        }
    }
}