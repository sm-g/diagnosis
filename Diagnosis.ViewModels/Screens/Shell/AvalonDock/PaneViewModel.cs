using Diagnosis.Common;
using System;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels.Screens
{
    public class PaneViewModel : SessionVMBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(PaneViewModel));

        private string _contentId = null;
        private bool _isActive = false;
        private bool _isSelected = false;
        private string _title = null;
        private Action<bool> OnIsAutoHiddenSet;

        /// <summary>
        /// Заголовок панели
        /// </summary>
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

        public RelayCommand NothingCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                }, () => false);
            }
        }

        /// <summary>
        /// Вместо IsAutoHidden depprop, которого нет
        /// </summary>
        public void SetIsAutoHidden(bool willBeAutoHidden)
        {
            if (OnIsAutoHiddenSet != null)
                OnIsAutoHiddenSet(willBeAutoHidden);
        }

        /// <summary>
        /// Чтобы управлять IsAutoHidden.
        /// </summary>
        public void SetIsAutoHiddenSettingCallback(Action<bool> actToNewValue)
        {
            if (OnIsAutoHiddenSet == null)
                this.OnIsAutoHiddenSet = actToNewValue;
        }

        public override string ToString()
        {
            return "pane " + Title;
        }
    }
}