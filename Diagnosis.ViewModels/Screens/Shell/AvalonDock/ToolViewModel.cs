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

        bool wasDeAutoHiddenBeforeHide;
        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    logger.DebugFormat("IsVisible {0} -> {1}", ContentId, value);

                    if (value)
                    {
                        // AD call LayoutAnchorable.Show() after
                        if (wasDeAutoHiddenBeforeHide)
                            AutoHide();
                    }
                    else
                    {
                        // AD call LayoutAnchorable.Hide() after
                        // to prevent empty LayoutAnchorGroup, call ToggleAutoHide() for him

                        //  wasDeAutoHiddenBeforeHide = IsAutoHidden;

                        // there is no callback before layout loaded
                        // ShowAutoHidden();
                    }

                    OnPropertyChanged("IsVisible");
                }
            }
        }

        public void Activate()
        {
            HideAfterInsert = false;
            IsVisible = true;
            IsActive = true;
            ShowAutoHidden();
        }

        public override string ToString()
        {
            return "tool " + Title;
        }
    }
}
