using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data;
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
    public class VocabularyListViewModel : ScreenBaseViewModel
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
        private string LocalConnectionString;
        private string LocalProviderName;

        public VocabularyListViewModel(ConnectionInfo server = null)
        {
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

                    var vms = SelectedAvailableVocs.ToList();
                    var selectedIds = vms.Select(x => x.voc.Id).ToList();

                    var syncer = new Syncer(
                         serverConStr: Remote.ConnectionString,
                         clientConStr: LocalConnectionString,
                         serverProviderName: Remote.ProviderName);

                    // только выбранные словари и всё для них
                    syncer.IdsToSyncPerType = new Dictionary<Type, IEnumerable<object>>();
                    syncer.IdsToSyncPerType.Add(typeof(Vocabulary), selectedIds.Cast<object>());
                    syncer.IdsToSyncPerType.Add(typeof(WordTemplate), vms.SelectMany(x => x.voc.WordTemplates.Select(y => y.Id)).Cast<object>());
                    syncer.IdsToSyncPerType.Add(typeof(Speciality), vms.SelectMany(x => x.voc.Specialities.Select(y => y.Id)).Cast<object>());
                    // syncer.IdsToSyncPerType.Add(typeof(SpecialityVocabularies), vms.SelectMany(x => x.voc.Specialities.Select(y => y.Id)).Cast<object>());

                    DoWithCursor(syncer.SendFrom(Side.Server, Scope.Voc.ToEnumerable()).ContinueWith((t) =>
                    {
                        var selectedSynced = Session.Query<Vocabulary>()
                            .Where(v => selectedIds.Contains(v.Id))
                            .ToList();

                        loader.LoadOrUpdateVocs(selectedSynced);
                    }
                    ).ContinueWith((t) =>
                    {
                        MakeInstalledVms();
                        MakeAvailableVms();
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
            var vocs = Session.Query<Vocabulary>()
                .ToList();
            var vms = vocs
                // .Where(x => doctor.Speciality != null ? doctor.Speciality.Vocabularies.Contains(x) : false)
                .Where(x => !x.IsCustom)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerConstrSettingName, Remote.ConnectionString }.AsParams(MessageKeys.Name, MessageKeys.Value));
                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerProviderSettingName, Remote.ProviderName }.AsParams(MessageKeys.Name, MessageKeys.Value));
            }
            base.Dispose(disposing);
        }
    }
}