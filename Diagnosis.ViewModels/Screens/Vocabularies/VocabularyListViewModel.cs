using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Diagnosis.ViewModels.Controls;
using Diagnosis.ViewModels.Search;
using EventAggregator;
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
    public class VocabularyListViewModel : ScreenBaseViewModel
    {
        private static NHibernateHelper nhib;
        private ObservableCollection<VocabularyViewModel> _vocs;
        private ObservableCollection<VocabularyViewModel> _availableVocs;
        private bool _noVocs;
        private bool _noNewVocs;
        private bool _connected;
        private DataConnectionViewModel _remote;
        private string LocalConnectionString;
        private string LocalProviderName;
        private Saver saver;
        private VocLoader loader;

        public VocabularyListViewModel()
        {
            Title = "Словари";
            saver = new Saver(Session);
            loader = new VocLoader(Session);

            SelectedVocs = new ObservableCollection<VocabularyViewModel>();
            SelectedAvailableVocs = new ObservableCollection<VocabularyViewModel>();

            LocalConnectionString = NHibernateHelper.Default.ConnectionString;
            LocalProviderName = LocalConnectionString.Contains(".sdf") ? Constants.SqlCeProvider : Constants.SqlServerProvider;

            var server = Constants.ServerConnectionInfo; // из Settings
            Remote = new DataConnectionViewModel(server);
            Remote.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "ConnectionString" || e.PropertyName == "ProviderName")
                {
                    TryGetAvailableVocs();
                }
            };

            MakeInstalledVms(Session.Query<Vocabulary>());
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

                    var vms = SelectedAvailableVocs.ToList();
                    var mergerdVocs = new List<Vocabulary>();
                    vms.ForEach(vm => mergerdVocs.Add(Session.Merge(vm.voc))); // new id! синхронизировать?
                    loader.LoadOrUpdateVocs(mergerdVocs);
                    vms.ForEach(vm => AvailableVocs.Remove(vm));

                    MakeInstalledVms(Session.Query<Vocabulary>());
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

                    MakeInstalledVms(Session.Query<Vocabulary>());
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
            var conn = new ConnectionInfo(Remote.ConnectionString, Remote.ProviderName);

            if (nhib == null || nhib.ConnectionString != conn.ConnectionString)
                nhib = NHibernateHelper.FromServerConnectionInfo(conn);

            var ids = Vocs.Select(y => y.voc.Id).ToList();
            try
            {
                using (var s = nhib.OpenSession())
                using (var tr = s.BeginTransaction())
                {
                    var vocs = s.Query<Vocabulary>()
                        .Where(x => !ids.Contains(x.Id))
                        .ToList();
                    MakeAvailableVms(vocs);
                    tr.Commit();
                }
                IsConnected = true;
                NoAvailableVocs = AvailableVocs.Count == 0;
            }
            catch (System.Exception)
            {
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
                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerConstrSettingName, Remote.ConnectionString }.AsParams(MessageKeys.Name, MessageKeys.Value));
                this.Send(Event.PushToSettings, new object[] { Constants.SyncServerProviderSettingName, Remote.ProviderName }.AsParams(MessageKeys.Name, MessageKeys.Value));
            }
            base.Dispose(disposing);
        }
    }
}