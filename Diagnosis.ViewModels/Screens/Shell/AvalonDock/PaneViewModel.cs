using Diagnosis.Common;
using System;

namespace Diagnosis.ViewModels.Screens
{
    public class PaneViewModel : SessionVMBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(PaneViewModel));

        private string _contentId = null;
        private bool _isActive = false;
        private bool _hide;
        //private bool _isAutoHidden;
        private bool _isSelected = false;
        private string _title = null;
        private Action<bool> OnIsAutoHiddenChanging;


        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        public string ContentId
        {
            get { return _contentId; }
            set
            {
                if (_contentId != value)
                {
                    _contentId = value;
                    OnPropertyChanged("ContentId");
                }
            }
        }

        public virtual Uri IconSource
        {
            get;

            protected set;
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    //logger.DebugFormat("{0} IsActive = {1}", this, value);
                    OnPropertyChanged("IsActive");
                }
            }
        }

        /// <summary>
        /// Anchorable will be autohidden after insert.
        /// </summary>
        public bool HideAfterInsert
        {
            get
            {
                return _hide;
            }
            set
            {
                if (_hide != value)
                {
                    _hide = value;
                    OnPropertyChanged(() => HideAfterInsert);
                }
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    //logger.DebugFormat("{0} IsSelected = {1}", this, value);
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        // public bool IsAutoHidden
        //{
        //    get
        //    {
        //        return _isAutoHidden;
        //    }
        //    set
        //    {
        //        if (_isAutoHidden != value)
        //        {
        //            _isAutoHidden = value;
        //            logger.DebugFormat("{0} IsAutoHidden = {1}", this, value);

        //            OnPropertyChanged(() => IsAutoHidden);
        //        }
        //    }
        //}

        public RelayCommand NothingCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                }, () => false);
            }
        }

        public void ShowAutoHidden()
        {
            if (OnIsAutoHiddenChanging != null)
                OnIsAutoHiddenChanging(false);
        }

        public void AutoHide()
        {
            if (OnIsAutoHiddenChanging != null)
                OnIsAutoHiddenChanging(true);
        }

        public void SetIsAutoHiddenChangingCallback(Action<bool> actToNewValue)
        {
            this.OnIsAutoHiddenChanging = actToNewValue;
        }

        public override string ToString()
        {
            return "pane " + Title;
        }
    }
}