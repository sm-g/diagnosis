﻿using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.ViewModels.Controls;
using NHibernate;
using System;
using System.Linq;
using System.Windows;

namespace Diagnosis.ViewModels.Screens
{
    public class OptionsLoaderViewModel : ViewModelBase
    {
        private string _buffer;
        private bool _part;
        private bool _showB;
        private bool _failed;
        private QueryEditorViewModel master;
        private OptionsLoader loader;
        private bool useBuffer;
        private VisibleRelayCommand _openBufferCommand;
        private EventAggregator.EventMessageHandler handler;
        private ISession session;

        public OptionsLoaderViewModel(QueryEditorViewModel queryeditor, OptionsLoader loader, ISession session, bool useBuffer = false)
        {
            this.master = queryeditor;
            this.useBuffer = useBuffer;
            this.loader = loader;
            this.session = session;

            handler = this.Subscribe(Event.NewSession, (e) =>
            {
                var s = e.GetValue<ISession>(MessageKeys.Session);
                ReplaceSession(s);
            });
        }

        public RelayCommand LoadOptionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    string str = GetStringToLoad();
                    var opt = loader.ReadOptions(str, session);

                    LoadFailed = opt == null;
                    if (opt != null)
                    {
                        master.SetOptions(opt);
                        PartialLoaded = opt.PartialLoaded;
                    }
                }, () => CanLoad());
            }
        }

        public RelayCommand SaveOptionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var options = master.GetOptions();
                    string str;
                    try
                    {
                        str = loader.WriteOptions(options);
                    }
                    catch (Exception ex)
                    {
                        str = ex.ToString();
                    }

                    ShowSavedString(str);
                }, () => !master.AllEmpty);
            }
        }

        public VisibleRelayCommand OpenBufferCommand
        {
            get
            {
                return _openBufferCommand ?? (_openBufferCommand = new VisibleRelayCommand(() =>
                {
                    ShowBuffer = true;
                }) { IsVisible = useBuffer });
            }
        }

        public RelayCommand HidePartialLoadedWarningCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    PartialLoaded = false;
                });
            }
        }

        public bool ShowBuffer
        {
            get
            {
                return _showB;
            }
            set
            {
                if (_showB != value)
                {
                    _showB = value;
                    OnPropertyChanged(() => ShowBuffer);
                }
            }
        }

        public string Buffer
        {
            get
            {
                return _buffer;
            }
            set
            {
                if (_buffer != value)
                {
                    _buffer = value;
                    OnPropertyChanged(() => Buffer);
                }
            }
        }

        public bool PartialLoaded
        {
            get
            {
                return _part;
            }
            set
            {
                if (_part != value)
                {
                    _part = value;
                    OnPropertyChanged(() => PartialLoaded);
                }
            }
        }

        public bool LoadFailed
        {
            get
            {
                return _failed;
            }
            set
            {
                if (_failed != value)
                {
                    _failed = value;
                    OnPropertyChanged(() => LoadFailed);
                }
            }
        }

        private bool CanLoad()
        {
            if (useBuffer)
            {
                return ShowBuffer;
            }
            return Clipboard.ContainsText();
        }
        private void ReplaceSession(ISession s)
        {
            if (this.session.SessionFactory == s.SessionFactory)
                this.session = s;
        }

        private string GetStringToLoad()
        {
            string str = null;
            if (useBuffer)
            {
                str = Buffer;
                Buffer = "";
                ShowBuffer = false;
            }
            else
            {
                var ido = Clipboard.GetDataObject();
                if (ido.GetDataPresent(DataFormats.UnicodeText))
                {
                    str = (string)ido.GetData(DataFormats.UnicodeText);
                }
            }
            return str;
        }

        private void ShowSavedString(string str)
        {
            if (useBuffer)
            {
                Buffer = str;
                ShowBuffer = true;
            }
            else
            {
                var dataObj = new DataObject(System.Windows.DataFormats.UnicodeText, str);
                Clipboard.SetDataObject(dataObj, true);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    handler.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

    }
}