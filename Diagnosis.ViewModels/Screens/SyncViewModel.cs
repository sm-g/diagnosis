using Diagnosis.Common;
using Diagnosis.Data.Sync;
using EventAggregator;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Diagnosis.ViewModels.Screens
{
    public class SyncViewModel : ScreenBaseViewModel
    {
        private string _log;

        public SyncViewModel()
        {
            Title = "Синхронизация";
            Syncer.MessagePosted += Syncer_MessagePosted;
            Syncer.SyncEnded += Syncer_SyncEnded;
        }

        public RelayCommand DownloadCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    DoWithCursor(Syncer.SendFrom(Db.Server), Cursors.AppStarting);
                },
                () => !Syncer.InSync);
            }
        }

        public RelayCommand UploadCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    DoWithCursor(Syncer.SendFrom(Db.Client), Cursors.AppStarting);
                },
                () => !Syncer.InSync);
            }
        }

        public RelayCommand DeprovisClientCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    DoWithCursor(Syncer.Deprovision(Db.Client), Cursors.AppStarting);
                },
                () => !Syncer.InSync);
            }
        }

        public RelayCommand DeprovisServerCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    DoWithCursor(Syncer.Deprovision(Db.Server), Cursors.AppStarting);
                },
                () => !Syncer.InSync);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Syncer.SyncEnded -= Syncer_SyncEnded;
                Syncer.MessagePosted -= Syncer_MessagePosted;
            }
            base.Dispose(disposing);
        }

        private void Syncer_MessagePosted(object sender, StringEventArgs e)
        {
            Log += e.str + '\n';
        }

        private void Syncer_SyncEnded(object sender, System.EventArgs e)
        {
            Log += "\nsync ended";
            CommandManager.InvalidateRequerySuggested();
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
    }
}