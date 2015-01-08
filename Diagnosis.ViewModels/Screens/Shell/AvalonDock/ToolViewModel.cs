using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Screens
{
    public class ToolViewModel : PaneViewModel
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(ToolViewModel));


        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    // logger.DebugFormat("IsVisible {0} -> {1}", ContentId, value);
                    OnPropertyChanged("IsVisible");
                }
            }
        }


        public override string ToString()
        {
            return "tool " + Title;
        }
    }
}
