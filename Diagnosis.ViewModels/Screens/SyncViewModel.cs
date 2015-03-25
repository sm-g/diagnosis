using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Diagnosis.ViewModels.Framework;
using EventAggregator;
using NHibernate.Linq;
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

            LocalConnectionString = NHibernateHelper.Default.ConnectionString;
            LocalProviderName = LocalConnectionString.Contains(".sdf") ? Constants.SqlCeProvider : Constants.SqlServerProvider;

            var server = Constants.ServerConnectionInfo; // из Settings
            OpenRemoteViewModel = new OpenRemoteViewModel(server);

            Syncer.MessagePosted += syncer_MessagePosted;
            Syncer.SyncEnded += syncer_SyncEnded;
        }
        private OpenRemoteViewModel _remote;
        public OpenRemoteViewModel OpenRemoteViewModel
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
                    OnPropertyChanged(() => OpenRemoteViewModel);
                }
            }
        }
        /// <summary>
        /// Remote, server or middle on Client.exe, middle on Server.exe
        /// </summary>
        public string RemoteConnectionString
        {
            get
            {
                return OpenRemoteViewModel.RemoteConnectionString;
            }
        }

        public string RemoteProviderName
        {
            get
            {
                return OpenRemoteViewModel.RemoteProviderName;
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
                            scopes = Scope.Reference.ToEnumerable();

                        // создаем промежуточную БД
                        SqlHelper.CreateSqlCeByConStr(sdfFileConstr);

                        DoWithCursor(
                            // выгрузка в существующую БД, которая может быть изменена после этого
                            // можно создавать файл заново
                            // или сначала депровизить, чтобы добавить данные к существующим
                            Syncer.Deprovision(sdfFileConstr, Constants.SqlCeProvider, scopes).ContinueWith(t =>
                            syncer.SendFrom(Side.Server, scopes).ContinueWith((t1) =>
                                // после выгрузки сразу готовим промежуточную БД к следующей синхронизации
                            Syncer.Deprovision(sdfFileConstr, Constants.SqlCeProvider, scopes))), Cursors.AppStarting);
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
                    var replacing = GetReplaceEntities<HrCategory>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateParents<HrCategory, HealthRecord>(replacing,
                            x => x.Category,
                            (x, value) => x.Category = value);

                        scopesToDeprovision.Add(typeof(HealthRecord).GetScope());
                    }
                    CleanupReplaced(replacing.Keys);
                }
                else if (type == typeof(Uom))
                {
                    var replacing = GetReplaceEntities<Uom>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateParents<Uom, HrItem>(replacing,
                            x => x.Measure != null ? x.Measure.Uom : null,
                            (x, value) => { if (x.Measure != null) x.Measure.Uom = value; });

                        scopesToDeprovision.Add(typeof(HrItem).GetScope());
                    }
                    CleanupReplaced(replacing.Keys);
                }
                else if (type == typeof(UomType))
                {
                    var replacing = GetReplaceEntities<UomType>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateParents<UomType, Uom>(replacing,
                            x => x.Type,
                            (x, value) => x.Type = value);

                        scopesToDeprovision.Add(typeof(Uom).GetScope());
                    }
                    CleanupReplaced(replacing.Keys);
                }
                else if (type == typeof(Speciality))
                {
                    var replacing = GetReplaceEntities<Speciality>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateParents<Speciality, Doctor>(replacing,
                            x => x.Speciality,
                            (x, value) => x.Speciality = value);
                        UpdateParents<Speciality, SpecialityIcdBlocks>(replacing,
                            x => x.Speciality,
                            (x, value) => x.Speciality = value);

                        scopesToDeprovision.Add(typeof(Doctor).GetScope());
                        scopesToDeprovision.Add(typeof(SpecialityIcdBlocks).GetScope());
                    }
                    CleanupReplaced(replacing.Keys);
                }
                else if (type == typeof(SpecialityIcdBlocks))
                {
                    var replacing = GetReplaceEntities<SpecialityIcdBlocks>(entities);
                    CleanupReplaced(replacing.Keys);
                    // нет ссылок на SpecialityIcdBlocks, нечего обновлять
                }
                else
                    throw new NotImplementedException();
            }

            // deprovision scopes обновленных сущностей
            Syncer.Deprovision(LocalConnectionString, LocalProviderName, scopesToDeprovision);
        }

        /// <summary>
        /// Возвращает сущности для замены с таким же значением. Cловарь со значениями { oldEntity, newEntity }
        /// </summary>
        /// <typeparam name="T">Тип сущности справочника для замены</typeparam>
        /// <param name="entities"></param>
        private Dictionary<T, T> GetReplaceEntities<T>(IList<object> entities)
            where T : IEntity
        {
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
        /// Меняем поле в сущностях для обновления.
        /// </summary>
        /// <typeparam name="T">Тип сущности справочника для замены</typeparam>
        /// <typeparam name="TUpdate">Тип сущности, в которой меняются сущности справочника для замены</typeparam>
        /// <param name="toReplace">Сущности для замены, значения { oldEntity, newEntity }</param>
        /// <param name="propertyGetter">Геттер свойства для обновления</param>
        /// <param name="propertySetter">Сеттер свойства для обновления</param>
        private void UpdateParents<T, TUpdate>(Dictionary<T, T> toReplace, Func<TUpdate, T> propertyGetter, Action<TUpdate, T> propertySetter)
            where T : IEntity
            where TUpdate : IEntity
        {
            var existing = Session.Query<TUpdate>()
                .ToList();
            var toUpdate = existing.Where(x => propertyGetter(x) != null && toReplace.Keys.Contains(propertyGetter(x)))
                .ToList();

            toUpdate.ForEach(x => propertySetter(x, toReplace[propertyGetter(x)]));

            // сохраняем обновленные
            new Saver(Session).Save(toUpdate.Cast<IEntity>().ToArray());
        }

        /// <summary>
        /// Удаляет замененные.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="replaced">Замененные сущности для удаления</param>
        private void CleanupReplaced(IEnumerable<IEntity> replaced)
        {
            var list = replaced.ToArray();
            new Saver(Session).Delete(list);

            if (list.Count() > 0)
                Log += string.Format("[{0:mm:ss:fff}] replaced {1} {2}\n", DateTime.Now, list.Count(), list.First().Actual.GetType().Name);
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