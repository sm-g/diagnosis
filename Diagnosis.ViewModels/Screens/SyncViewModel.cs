using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Diagnosis.ViewModels.Framework;
using EventAggregator;
using NHibernate.Linq;
using System;

using System;

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Diagnosis.ViewModels.Screens
{
    public class SyncViewModel : ScreenBaseViewModel
    {
        private static string _log;

        private string _remoteConStr;
        private string _remoteProvider;
        private string _localConStr;
        private string _localProvider;

        public SyncViewModel()
        {
            Title = "Синхронизация";

            LocalConnectionString = NHibernateHelper.ConnectionString;
            LocalProviderName = LocalConnectionString.Contains(".sdf") ? Constants.SqlCeProvider : Constants.SqlServerProvider;

            var server = Constants.ServerConnectionInfo; // из Settings
            if (server != null)
            {
                RemoteConnectionString = server.ConnectionString;
                RemoteProviderName = server.ProviderName;
            }
#if DEBUG
            //    RemoteConnectionString = "Data Source=remote.sdf";
            RemoteProviderName = Constants.SqlCeProvider;
#endif
            Syncer.MessagePosted += syncer_MessagePosted;
            Syncer.SyncEnded += syncer_SyncEnded;
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

        /// <summary>
        /// Open remote SqlCe DB.
        /// </summary>
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
                        RemoteProviderName = Constants.SqlCeProvider;
                    }
                });
            }
        }

        /// <summary>
        /// Загружает справочные данные с удаленной серверной БД на клиент.
        /// </summary>
        public RelayCommand DownloadCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Contract.Requires(LocalProviderName == Constants.SqlCeProvider); // загружаем только на клиента

                    var syncer = new Syncer(
                         serverConStr: RemoteConnectionString,
                         clientConStr: LocalConnectionString,
                         serverProviderName: RemoteProviderName);

                    DoWithCursor(syncer.SendFrom(Side.Server).ContinueWith((t) =>
                        // после загрузки проверяем справочные сущности на совпадение
                        CheckReferenceEntitiesAfterDownload(syncer.AddedIdsPerType)), Cursors.AppStarting);
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
                return new RelayCommand(() =>
                {
                    var result = new FileDialogService().ShowSaveFileDialog(null,
                         FileType.Sdf.ToEnumerable(),
                         FileType.Sdf,
                         "diagnosis-ref");

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
                            scopes = Scope.Reference.ToEnumerable();

                        // создаем промежуточную БД
                        SqlHelper.CreateSqlCeByConStr(sdfFileConstr);

                        DoWithCursor(
                            syncer.SendFrom(Side.Server, scopes).ContinueWith((t) =>
                                // после выгрузки сразу готовим промежуточную БД к следующей синхронизации
                            Syncer.Deprovision(sdfFileConstr, Constants.SqlCeProvider, scopes)), Cursors.AppStarting);
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
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                CommandManager.InvalidateRequerySuggested()));
        }

        private void CheckReferenceEntitiesAfterDownload(Dictionary<Type, IEnumerable<object>> addedIdsPerType)
        {
            Contract.Requires(addedIdsPerType != null);
            var scopesToDeprovision = new HashSet<Scope>();
            var types = addedIdsPerType.Keys
                .OrderByDescending(x => x, new RefModelsComparer()); // сначала родители

            // дети обновляются на нового родителя
            // старый родитель удаляется без каскадного удаления детей
            // потом удаляются дети

            foreach (var type in types)
            {
                var ids = addedIdsPerType[type];
                var entities = ids.Select(id => Session.Get(type, id)).ToList();

                // проверяем справочные сущности на совпадение
                if (type == typeof(HrCategory))
                {
                    var replaced = ReplaceRefEntities<HrCategory>(entities);
                    if (replaced.Count > 0)
                    {
                        UpdateParents<HrCategory, HealthRecord>(replaced,
                            x => x.Category,
                            (x, value) => x.Category = value);

                        scopesToDeprovision.Add(typeof(HealthRecord).GetScope());
                    }
                    CleanupReplaced(replaced);
                }
                else if (type == typeof(Uom))
                {
                    var replaced = ReplaceRefEntities<Uom>(entities);
                    if (replaced.Count > 0)
                    {
                        UpdateParents<Uom, HrItem>(replaced,
                            x => x.Measure != null ? x.Measure.Uom : null,
                            (x, value) => x.Measure.Uom = value);

                        scopesToDeprovision.Add(typeof(HrItem).GetScope());
                    }
                    CleanupReplaced(replaced);
                }
                else if (type == typeof(UomType))
                {
                    var replaced = ReplaceRefEntities<UomType>(entities);
                    if (replaced.Count > 0)
                    {
                        UpdateParents<UomType, Uom>(replaced,
                            x => x.Type,
                            (x, value) => x.Type = value);

                        scopesToDeprovision.Add(typeof(Uom).GetScope());
                    }
                    CleanupReplaced(replaced);
                }
                else if (type == typeof(Speciality))
                {
                    var replaced = ReplaceRefEntities<Speciality>(entities);
                    if (replaced.Count > 0)
                    {
                        UpdateParents<Speciality, Doctor>(replaced,
                            x => x.Speciality,
                            (x, value) => x.Speciality = value);
                        UpdateParents<Speciality, SpecialityIcdBlocks>(replaced,
                            x => x.Speciality,
                            (x, value) => x.Speciality = value);

                        scopesToDeprovision.Add(typeof(Doctor).GetScope());
                        scopesToDeprovision.Add(typeof(SpecialityIcdBlocks).GetScope());
                    }
                    CleanupReplaced(replaced);
                }
                else if (type == typeof(SpecialityIcdBlocks))
                {
                    var replaced = ReplaceRefEntities<SpecialityIcdBlocks>(entities);
                    CleanupReplaced(replaced);
                    // нет ссылок на SpecialityIcdBlocks, нечего обновлять
                }
                else
                    throw new NotImplementedException();
            }

            // deprovision scopes обновленных сущностей
            Syncer.Deprovision(LocalConnectionString, LocalProviderName, scopesToDeprovision);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T">Тип сущности справочника для замены</typeparam>
        /// <param name="entities"></param>
        private Dictionary<T, T> ReplaceRefEntities<T>(IList<object> entities)
            where T : IEntity
        {
            // ищем сущности для замены с таким же значением
            var toReplace = new Dictionary<T, T>();

            foreach (var item in entities.Cast<T>())
            {
                var existing = Session.Query<T>()
                    .Where(x => x.Id != item.Id)
                    .Where(IEntityExtensions.EqualsByVal(item))
                    .FirstOrDefault();

                if (existing != null)
                    toReplace[existing] = item;
            }

            return toReplace;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T">Тип сущности справочника для замены</typeparam>
        /// <typeparam name="TUpdate">Тип сущности, в которой меняются сущности справочника для замены</typeparam>
        /// <param name="toReplace"></param>
        /// <param name="propertyGetter">Геттер свойства для обновления</param>
        /// <param name="propertySetter">Сеттер свойства для обновления</param>
        private void UpdateParents<T, TUpdate>(Dictionary<T, T> toReplace, Func<TUpdate, T> propertyGetter, Action<TUpdate, T> propertySetter)
            where T : IEntity
            where TUpdate : IEntity
        {
            // меняем поле в сущностях для обновления
            var toUpdate = Session.Query<TUpdate>()
                .ToList()
                .Where(x => propertyGetter(x) != null && toReplace.Keys.Contains(propertyGetter(x)))
                .ToList();

            toUpdate.ForEach(x => propertySetter(x, toReplace[propertyGetter(x)]));

            // сохраняем обновленные
            new Saver(Session).Save(toUpdate.Cast<IEntity>().ToArray());
        }

        private void CleanupReplaced<T>(Dictionary<T, T> replaced) where T : IEntity
        {
            new Saver(Session).Delete(replaced.Keys.Cast<IEntity>().ToArray());

            if (replaced.Count > 0)
                Log += string.Format("[{0:mm:ss:fff}] replaced {1} {2}\n", DateTime.Now, replaced.Count, replaced.Keys.First().Actual.GetType().Name);
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

                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerConstrSettingName, RemoteConnectionString }.AsParams(MessageKeys.Name, MessageKeys.Value));
                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerProviderSettingName, RemoteProviderName }.AsParams(MessageKeys.Name, MessageKeys.Value));
            }
            base.Dispose(disposing);
        }
    }
}