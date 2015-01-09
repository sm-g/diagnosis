﻿using Diagnosis.Common;
using System;

namespace Diagnosis.ViewModels.Screens
{
    public class PaneViewModel : SessionVMBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(PaneViewModel));

        private string _contentId = null;
        private bool _isActive = false;
        private bool _hide;
        private bool _isAutoHidden;
        private bool _isSelected = false;
        private string _title = null;
        public event EventHandler<BoolEventArgs> IsAutoHiddenChanging;


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

        public RelayCommand NoAutoHideCommand
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
            OnIsAutoHiddenChanging(false);
        }

        public void AutoHide()
        {
            OnIsAutoHiddenChanging(true);
        }

        public override string ToString()
        {
            return "pane " + Title;
        }

        protected virtual void OnIsAutoHiddenChanging(bool value)
        {
            var h = IsAutoHiddenChanging;
            if (h != null)
            {
                h(this, new BoolEventArgs(value));
            }
        }
    }
}