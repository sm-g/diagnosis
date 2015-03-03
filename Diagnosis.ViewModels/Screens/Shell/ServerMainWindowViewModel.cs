using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class ServerMainWindowViewModel : ViewModelBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(ServerMainWindowViewModel));

        public ServerMainWindowViewModel(bool demoMode = false)
        {
            this.Subscribe(Event.EditUom, (e) =>
                        {
                            var uom = e.GetValue<Uom>(MessageKeys.Uom);
                            IDialogViewModel vm = new UomEditorViewModel(uom);
                            this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
                        });
            var prefix = demoMode ? "Демо :: " : "";
            Title = prefix + "Diagnosis on server";
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(() => Title);
                }
            }
        }
    }
}