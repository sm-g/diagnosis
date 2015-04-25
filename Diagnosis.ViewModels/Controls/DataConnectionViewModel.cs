using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.ViewModels.Framework;
using EventAggregator;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Controls
{
    public class DataConnectionViewModel : ViewModelBase
    {
        private string _remoteConStr;
        private string _remoteProvider;

        public DataConnectionViewModel()
        {

        }

        public DataConnectionViewModel(ConnectionInfo initial)
        {
            ConnectionString = initial.ConnectionString;
            ProviderName = initial.ProviderName;
#if DEBUG

            ProviderName = Constants.SqlCeProvider;
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
                        ConnectionString = result.FileName;
                        ProviderName = Constants.SqlCeProvider;
                    }
                });
            }
        }

        public string ConnectionString
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
                    OnPropertyChanged(() => ConnectionString);
                }
            }
        }

        public string ProviderName
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
                    OnPropertyChanged(() => ProviderName);
                }
            }
        }

        public ConnectionInfo ConnectionInfo
        {
            get { return new ConnectionInfo(ConnectionString, ProviderName); }
        }
    }
}