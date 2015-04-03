using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Diagnosis.ViewModels.Controls;
using EventAggregator;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class VocabularySyncViewModel : ScreenBaseViewModel
    {
        private static NHibernateHelper nhib;
        private ObservableCollection<VocabularyViewModel> _vocs;
        private ObservableCollection<VocabularyViewModel> _availableVocs;
        private bool _noNewVocs;
        private bool _connected;
        private DataConnectionViewModel _remote;
        private Saver saver;
        private VocLoader loader;
        private List<Vocabulary> serverVocs = new List<Vocabulary>();
        private string _log;
        private string LocalConnectionString;
        private string LocalProviderName;

        public VocabularySyncViewModel(ConnectionInfo server = null)
        {
            Contract.Requires(Constants.IsClient);

            Title = "Словари";
            saver = new Saver(Session);
            loader = new VocLoader(Session);

            LocalConnectionString = NHibernateHelper.Default.ConnectionString;
            LocalProviderName = LocalConnectionString.Contains(".sdf") ? Constants.SqlCeProvider : Constants.SqlServerProvider;

            SelectedVocs = new ObservableCollection<VocabularyViewModel>();
            SelectedAvailableVocs = new ObservableCollection<VocabularyViewModel>();

            Remote = new DataConnectionViewModel(server);
            Remote.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "ConnectionString" || e.PropertyName == "ProviderName")
                {
                    TryGetAvailableVocs();
                }
            };
            Syncer.MessagePosted += syncer_MessagePosted;
            Syncer.SyncEnded += syncer_SyncEnded;

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
                    Contract.Requires(Constants.IsClient);

                    var vocsToLoad = SelectedAvailableVocs.Select(x => x.voc).ToList();
                    var selectedVocIds = vocsToLoad.Select(x => x.Id).ToList(); // for session
                    var selectedWtIds = vocsToLoad.SelectMany(x => x.WordTemplates.Select(y => y.Id));
                    var selectedSpecIds = vocsToLoad.SelectMany(x => x.Specialities.Select(y => y.Id));
                    var selectedSpecVocIds = vocsToLoad.SelectMany(x => x.SpecialityVocabularies.Select(y => y.Id));

                    var syncer = new Syncer(
                         serverConStr: Remote.ConnectionString,
                         clientConStr: LocalConnectionString,
                         serverProviderName: Remote.ProviderName);

                    // только выбранные словари и всё для них с сервера. слова словаря не загружаются с сервера
                    syncer.IdsToSyncPerType = new Dictionary<Type, IEnumerable<object>>(){
                       {typeof(Vocabulary),             selectedVocIds.Cast<object>()},
                       {typeof(WordTemplate),           selectedWtIds.Cast<object>()},
                       {typeof(Speciality),             selectedSpecIds.Cast<object>()},
                       {typeof(SpecialityVocabularies), selectedSpecVocIds.Cast<object>()},
                    };
                    try
                    {
                        using (var s = nhib.OpenSession())
                        using (var tr = s.BeginTransaction())
                        {
                            // повторно загружаем даже если не было изменений на сервере
                            foreach (var id in selectedVocIds)
                                s.FakeUpdate(typeof(Vocabulary), id);
                            foreach (var id in selectedWtIds)
                                s.FakeUpdate(typeof(WordTemplate), id);
                            foreach (var id in selectedSpecVocIds)
                                s.FakeUpdate(typeof(SpecialityVocabularies), id);
                            // специальность не удаляется при удалении словаря
                            tr.Commit();
                        }
                    }
                    catch (Exception)
                    {
                        return;
                    }

                    DoWithCursor(syncer.SendFrom(Side.Server, Scope.Voc.ToEnumerable()).ContinueWith((t) =>
                    {
                        var selectedSynced = Session.Query<Vocabulary>()
                            .Where(v => selectedVocIds.Contains(v.Id))
                            .ToList();

                        loader.LoadOrUpdateVocs(selectedSynced);
                    }
                    ).ContinueWith((t) =>
                    {
                        MakeInstalledVms();
                        MakeAvailableVms();
#if DEBUG
                        AuthorityController.LoadVocsAfterLogin(Session); // загружаем словари не меняя пользователя
#endif
                    })
                    , Cursors.AppStarting);
                }, () => SelectedAvailableVocs.Count > 0 && IsConnected);
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

                    loader.DeleteVocs(toDel);

                    MakeInstalledVms();
                    MakeAvailableVms();
                }, () => SelectedVocs.Count > 0);
            }
        }

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

        private void TryGetAvailableVocs()
        {
            // подкключаемся к источнику
            var conn = new ConnectionInfo(Remote.ConnectionString, Remote.ProviderName);

            if (nhib == null || nhib.ConnectionString != conn.ConnectionString)
                nhib = NHibernateHelper.FromServerConnectionInfo(conn);

            MakeInstalledVms();
            try
            {
                using (var s = nhib.OpenSession())
                using (var tr = s.BeginTransaction())
                {
                    serverVocs = s.Query<Vocabulary>()
                       .ToList();

                    MakeAvailableVms();
                    tr.Commit();
                }
                IsConnected = true;
                NoAvailableVocs = AvailableVocs.Count == 0;
            }
            catch (System.Exception)
            {
                serverVocs.Clear();
                MakeAvailableVms();
                IsConnected = false;
                NoAvailableVocs = false; // пока нет подключения, этого сообщения нет
            }
        }

        private void MakeInstalledVms()
        {
            var vms = VocabularyQuery.NonCustom(Session)()
                .Select(voc => Vocs
                    .Where(vm => vm.voc == voc)
                    .FirstOrDefault() ?? new VocabularyViewModel(voc))
                .ToList();

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                Vocs.SyncWith(vms);
            }));
        }

        private void MakeAvailableVms()
        {
            var ids = Session.Query<Vocabulary>().Select(y => y.Id).ToList();
            var notInstalled = serverVocs
                .Where(x => !ids.Contains(x.Id));

            var vms = notInstalled.Select(voc => AvailableVocs
                    .Where(vm => vm.voc == voc)
                    .FirstOrDefault() ?? new VocabularyViewModel(voc))
                .ToList();

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                AvailableVocs.SyncWith(vms);
            }));
        }
        private void syncer_MessagePosted(object sender, StringEventArgs e)
        {
            Log += string.Format("[{0:mm:ss:fff}] {1}\n", DateTime.Now, e.str);
        }

        private void syncer_SyncEnded(object sender, TimeSpanEventArgs e)
        {
            Log += "\n=== " + e.ts.ToString() + "\n";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Syncer.SyncEnded -= syncer_SyncEnded;
                Syncer.MessagePosted -= syncer_MessagePosted;

                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerConstrSettingName, Remote.ConnectionString }.AsParams(MessageKeys.Name, MessageKeys.Value));
                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerProviderSettingName, Remote.ProviderName }.AsParams(MessageKeys.Name, MessageKeys.Value));
            }
            base.Dispose(disposing);
        }
    }
}