using AutoMapper;
using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.DataTransfer;
using Diagnosis.ViewModels.Search;
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
        private QueryEditorViewModel master;
        private OptionsLoader loader;
        bool useBuffer;
        private VisibleRelayCommand _openBufferCommand;

        public OptionsLoaderViewModel(QueryEditorViewModel s, OptionsLoader loader, bool useBuffer = false)
        {
            this.master = s;
            this.useBuffer = useBuffer;
            this.loader = loader;
        }

        public RelayCommand LoadOptionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    string str = GetStringToLoad();
                    var opt = loader.ReadOptions(str);
                    master.SetOptions(opt);
                    PartialLoaded = opt.PartialLoaded;

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

        private bool CanLoad()
        {
            if (useBuffer)
            {
                return ShowBuffer;
            }
            return Clipboard.ContainsText();
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
    }
}