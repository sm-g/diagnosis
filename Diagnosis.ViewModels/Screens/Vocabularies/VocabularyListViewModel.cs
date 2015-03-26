using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Diagnosis.ViewModels.Controls;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using NHibernate;
using NHibernate.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class VocabularyListViewModel : ScreenBaseViewModel, IFilterableList
    {
        private ObservableCollection<VocabularyViewModel> _vocs;
        private ObservableCollection<VocabularyViewModel> _availableVocs;
        private bool _noVocs;
        private Saver saver;
        private bool _noNewVocs;
        private ISession remoteSession;

        private DataConnectionViewModel _remote;

        private bool _connected;
        private string LocalConnectionString;
        private string LocalProviderName;
        private static NHibernateHelper nhib;

        public VocabularyListViewModel()
        {
            Title = "Словари";
            saver = new Saver(Session);
            SelectedVocs = new ObservableCollection<VocabularyViewModel>();
            SelectedAvailableVocs = new ObservableCollection<VocabularyViewModel>();

            MakeInstalledVms(Session.Query<Vocabulary>());
            //NoVocs = Vocs.Count == 0;

            var server = Constants.ServerConnectionInfo; // из Settings
            Remote = new DataConnectionViewModel(server);
            Remote.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "ConnectionString" || e.PropertyName == "ProviderName")
                {
                    TryGetAvailableVocs();
                }
            };

            LocalConnectionString = NHibernateHelper.Default.ConnectionString;
            LocalProviderName = LocalConnectionString.Contains(".sdf") ? Constants.SqlCeProvider : Constants.SqlServerProvider;

            TryGetAvailableVocs();
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

        public ObservableCollection<VocabularyViewModel> Vocs
        {
            get
            {
                if (_vocs == null)
                {
                    _vocs = new ObservableCollection<VocabularyViewModel>();
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(_vocs);
                    SortDescription sort1 = new SortDescription("Title", ListSortDirection.Ascending);
                    view.SortDescriptions.Add(sort1);
                }
                return _vocs;
            }
        }

        /// <summary>
        /// Новые словари, remote session
        /// </summary>
        public ObservableCollection<VocabularyViewModel> AvailableVocs
        {
            get
            {
                if (_availableVocs == null)
                {
                    _availableVocs = new ObservableCollection<VocabularyViewModel>();
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(_availableVocs);
                    SortDescription sort1 = new SortDescription("Title", ListSortDirection.Ascending);
                    view.SortDescriptions.Add(sort1);
                }
                return _availableVocs;
            }
        }

        public ObservableCollection<VocabularyViewModel> SelectedVocs { get; private set; }

        /// <summary>
        /// Выбранные новые словари, remote session
        /// </summary>
        public ObservableCollection<VocabularyViewModel> SelectedAvailableVocs { get; private set; }

        /// <summary>
        /// Устанавливает выбранные доступные словари.
        /// </summary>
        public RelayCommand LoadVocsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Contract.Requires(LocalProviderName == Constants.SqlCeProvider); // загружаем только на клиента
                    Contract.Requires(IsConnected);

                    var syncer = new Syncer(
                         serverConStr: Remote.ConnectionString,
                         clientConStr: LocalConnectionString,
                         serverProviderName: Remote.ProviderName);

                    // синхронизируем словари
                    //DoWithCursor(syncer.SendFrom(Side.Server, Scope.Voc.ToEnumerable()).ContinueWith((t) =>
                    //    // после загрузки проверяем словари
                    //    //    CheckReferenceEntitiesAfterDownload(syncer.AddedIdsPerType)).ContinueWith((t) =>
                    //new VocLoader(Session).ProceedDeletedOnServerVocs(syncer.DeletedIdsPerType))
                    //, Cursors.AppStarting);

                    // делаем слова из выбранных
                    var loader = new VocLoader(Session);
                    foreach (var voc in SelectedAvailableVocs.Select(x => x.voc))
                    {
                        remoteSession.Evict(voc);
                        Session.Merge(voc);

                        loader.LoadVoc(voc);
                    }
                    MakeInstalledVms(Session.Query<Vocabulary>());
                }, () => SelectedAvailableVocs.Count > 0);
            }
        }
        /// <summary>
        /// Обновляет выбранные установленные словари.
        /// </summary>
        public RelayCommand ReloadVocsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    // это же при синхронизации для всех словарей?
                    var loader = new VocLoader(Session);
                    foreach (var voc in SelectedAvailableVocs.Select(x => x.voc))
                    {
                        remoteSession.Evict(voc);
                        Session.Merge(voc);

                        loader.UpdateVoc(voc);
                    }

                }, () => SelectedVocs.Count > 0);
            }
        }
        /// <summary>
        /// Удаляет выбранные установленные словари.
        /// </summary>
        public ICommand DeleteVocsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var toDel = SelectedVocs
                        .Select(w => w.voc)
                        .ToArray();
                    var loader = new VocLoader(Session);

                    loader.DeleteVocs(toDel);

                    MakeInstalledVms(Session.Query<Vocabulary>());


                    //NoVocs = !Session.Query<Vocabulary>().Any();
                }, () => SelectedVocs.Count > 0);
            }
        }

        ///// <summary>
        ///// Нет установленных словарей.
        ///// </summary>
        //public bool NoVocs
        //{
        //    get
        //    {
        //        return _noVocs;
        //    }
        //    set
        //    {
        //        if (_noVocs != value)
        //        {
        //            _noVocs = value;
        //            OnPropertyChanged(() => NoVocs);
        //        }
        //    }
        //}

        /// <summary>
        /// Нет новых словарей на сервере.
        /// </summary>
        public bool NoAvailableVocs
        {
            get
            {
                return _noNewVocs;
            }
            set
            {
                if (_noNewVocs != value)
                {
                    _noNewVocs = value;
                    OnPropertyChanged(() => NoAvailableVocs);
                }
            }
        }
        /// <summary>
        /// Не подключен источник словарей.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _connected;
            }
            set
            {
                if (_connected != value)
                {
                    _connected = value;
                    OnPropertyChanged(() => IsConnected);
                }
            }
        }
        private void TryGetAvailableVocs()
        {
            // подкключаемся к источнику
            if (remoteSession != null)
                remoteSession.Dispose();

            var conn = new ConnectionInfo(Remote.ConnectionString, Remote.ProviderName);

            if (nhib == null || nhib.ConnectionString != conn.ConnectionString)
                nhib = NHibernateHelper.FromServerConnectionInfo(conn);

            var ids = Vocs.Select(y => y.voc.Id).ToList();
            try
            {
                remoteSession = nhib.OpenSession();
                using (var tr = remoteSession.BeginTransaction())
                {

                    var vocs = remoteSession.Query<Vocabulary>()
                        .Where(x => !ids.Contains(x.Id))
                        .ToList();
                    MakeAvailableVms(vocs);
                }
                IsConnected = true;
                NoAvailableVocs = AvailableVocs.Count == 0;
            }
            catch (System.Exception)
            {
                remoteSession = null;
                MakeAvailableVms(Enumerable.Empty<Vocabulary>());
                IsConnected = false;
                NoAvailableVocs = false; // пока нет подключения, этого сообщения нет
            }

        }
        private void MakeInstalledVms(IEnumerable<Vocabulary> results)
        {
            var vms = results.Select(w => Vocs
                .Where(vm => vm.voc == w)
                .FirstOrDefault() ?? new VocabularyViewModel(w));

            Vocs.SyncWith(vms);
        }

        private void MakeAvailableVms(IEnumerable<Vocabulary> results)
        {
            var vms = results.Select(w => AvailableVocs
                .Where(vm => vm.voc == w)
                .FirstOrDefault() ?? new VocabularyViewModel(w));

            AvailableVocs.SyncWith(vms);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //  emhManager.Dispose();
                if (remoteSession != null)
                    remoteSession.Dispose();

                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerConstrSettingName, Remote.ConnectionString }.AsParams(MessageKeys.Name, MessageKeys.Value));
                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerProviderSettingName, Remote.ProviderName }.AsParams(MessageKeys.Name, MessageKeys.Value));
            }
            base.Dispose(disposing);
        }
    }
}