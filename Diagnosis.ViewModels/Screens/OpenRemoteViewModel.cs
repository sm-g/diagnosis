using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.ViewModels.Framework;
using EventAggregator;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class OpenRemoteViewModel : ViewModelBase
    {
        private string _remoteConStr;
        private string _remoteProvider;

        public OpenRemoteViewModel(ConnectionInfo initial)
        {
            if (initial != null)
            {
                RemoteConnectionString = initial.ConnectionString;
                RemoteProviderName = initial.ProviderName;
            }
#if DEBUG

            RemoteProviderName = Constants.SqlCeProvider;
#endif
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
    }
}