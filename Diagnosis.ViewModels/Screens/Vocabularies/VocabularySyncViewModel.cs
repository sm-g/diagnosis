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
using System.Threading.Tasks;
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
        private List<Vocabulary> serverNonCustomVocs = new List<Vocabulary>();
        private static string _log;
        private string LocalConnectionString;
        private string LocalProviderName;

        public VocabularySyncViewModel(ConnectionInfo server = new ConnectionInfo())
        {
            Contract.Requires(Constants.IsClient);

            Title = "Словари";

            LocalConnectionString = Nhib.ConnectionString;
            LocalProviderName = LocalConnectionString.Contains(".sdf") ? Constants.SqlCeProvider : Constants.SqlServerProvider;

            Remote = new DataConnectionViewModel(server);
            Remote.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "ConnectionString" || e.PropertyName == "ProviderName")
                {
                    TryGetAvailableVocs();
                }
            };
            Poster.MessagePosted += syncer_MessagePosted;
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

        public IEnumerable<VocabularyViewModel> SelectedVocs { get { return Vocs.Where(x => x.IsSelected); } }

        /// <summary>
        /// Выбранные новые словари, remote session
        /// </summary>
        public IEnumerable<VocabularyViewModel> SelectedAvailableVocs { get { return AvailableVocs.Where(x => x.IsSelected); } }

        /// <summary>
        /// Устанавливает выбранные доступные словари.
        /// </summary>
        public RelayCommand LoadVocsCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    var vocsToLoad = SelectedAvailableVocs.Select(x => x.voc).ToList();

                    try
                    {
                        await SyncVocsToLoad(Remote.ConnectionInfo,
                            LocalConnectionString,
                            vocsToLoad);
                    }
                    catch (Exception e)
                    {
                        Log += e + Environment.NewLine;
                    }

                    AfterLoad(vocsToLoad);

                }, () => SelectedAvailableVocs.Any() && IsConnected);
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
                    var toDel = SelectedVocs.Select(w => w.voc).ToList();

                    DeleteVocs(toDel);
                }, () => SelectedVocs.Any());
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

        public NHibernateHelper NHib
        {
            get
            {
                var conn = Remote.ConnectionInfo;
                if (nhib == null || nhib.ConnectionString != conn.ConnectionString)
                    // подключаемся к источнику
                    nhib = NHibernateHelper.FromConnectionInfo(conn, Side.Server);

                return nhib;
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

        public async Task SyncVocsToLoad(ConnectionInfo remote, string local, IEnumerable<Vocabulary> vocsToLoad)
        {
            Contract.Requires(Constants.IsClient);

            var syncer = new Syncer(
                 serverConStr: remote.ConnectionString,
                 clientConStr: local,
                 serverProviderName: remote.ProviderName);

            using (var s = NHib.OpenSession())
                syncer = syncer.OnlySelectedVocs(s, vocsToLoad);

            Mouse.OverrideCursor = Cursors.AppStarting;
            await syncer.SendFrom(Side.Server, Scope.Voc);
            Mouse.OverrideCursor = null;
        }

        public void AfterLoad(IEnumerable<Vocabulary> vocsToLoad)
        {
            var ids = vocsToLoad.Select(x => x.Id).ToList();
            var selectedSynced = VocabularyQuery.ByIds(Session)(ids);

            new VocLoader(Session).LoadOrUpdateVocs(selectedSynced);

            MakeInstalledVms();
            MakeAvailableVms();
#if DEBUG
            AuthorityController.LoadVocsAfterLogin(); // загружаем словари не меняя пользователя
#endif
        }

        public void DeleteVocs(IEnumerable<Vocabulary> toDel)
        {
            new VocLoader(Session).DeleteVocs(toDel);

            MakeInstalledVms();
            MakeAvailableVms();

#if DEBUG
            AuthorityController.LoadVocsAfterLogin(); // обновляем доступные слова не меняя пользователя
#endif
        }


        private void TryGetAvailableVocs()
        {
            MakeInstalledVms();
            try
            {
                int available;
                using (var s = NHib.OpenSession())
                {
                    serverNonCustomVocs = VocabularyQuery.NonCustom(s)()
                       .ToList();

                    available = MakeAvailableVms();
                }
                IsConnected = true;
                NoAvailableVocs = available == 0;
            }
            catch (System.Exception)
            {
                serverNonCustomVocs.Clear();
                MakeAvailableVms();
                IsConnected = false;
                NoAvailableVocs = false; // пока нет подключения, этого сообщения нет
            }
        }
        private int MakeInstalledVms()
        {
            var vms = VocabularyQuery.NonCustom(Session)()
                .Select(voc => Vocs
                    .Where(vm => vm.voc == voc)
                    .FirstOrDefault() ?? new VocabularyViewModel(voc))
                .ToList();

            uiTaskFactory.StartNew(() =>
                Vocs.SyncWith(vms));

            return vms.Count();
        }

        private int MakeAvailableVms()
        {
            var ids = EntityQuery<Vocabulary>.All(Session)().Select(y => y.Id).ToList();
            var notInstalled = serverNonCustomVocs
                .Where(x => !ids.Contains(x.Id));

            var vms = notInstalled.Select(voc => AvailableVocs
                    .Where(vm => vm.voc == voc)
                    .FirstOrDefault() ?? new VocabularyViewModel(voc))
                .ToList();

            uiTaskFactory.StartNew(() =>
               AvailableVocs.SyncWith(vms));

            return vms.Count();
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
                Poster.MessagePosted -= syncer_MessagePosted;
                uiTaskFactory.StartNew(() =>
                {
                    Vocs.Clear();
                    AvailableVocs.Clear();
                });

                Remote.Dispose();
                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerConstrSettingName, Remote.ConnectionString }.AsParams(MessageKeys.Name, MessageKeys.Value));
                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerProviderSettingName, Remote.ProviderName }.AsParams(MessageKeys.Name, MessageKeys.Value));
            }
            base.Dispose(disposing);
        }
    }
}