using System;
using System.Windows.Input;
using EventAggregator;

namespace Diagnosis.App.ViewModels
{
    public class EditableBase : ViewModelBase, IEditable
    {
        private ICommand _commit;
        private ICommand _delete;
        private ICommand _edit;
        private ICommand _revert;
        private bool _editActive;
        private bool _editorFocused;
        private bool _canBeDirty;
        bool _isDirty;
        private bool _switchedOn;
        private bool _canBeDeleted;

        ViewModelBase vm;

        #region IEditable

        public event EditableEventHandler Committed;

        public event EditableEventHandler Deleted;

        public event EditableEventHandler ModelPropertyChanged;

        public bool IsEditorActive
        {
            get
            {
                return _editActive;
            }
            set
            {
                if (_editActive != value)
                {
                    _editActive = value;
                    OnPropertyChanged(() => IsEditorActive);
                }
            }
        }

        public bool IsEditorFocused
        {
            get
            {
                return _editorFocused;
            }
            set
            {
                if (_editorFocused != value)
                {
                    _editorFocused = value;
                    OnPropertyChanged(() => IsEditorFocused);
                }
            }
        }

        /// <summary>
        /// Allows to perform commands.
        /// </summary>
        public bool SwitchedOn
        {
            get
            {
                return _switchedOn;
            }
            set
            {
                if (_switchedOn != value)
                {
                    _switchedOn = value;
                    OnPropertyChanged(() => SwitchedOn);
                }
            }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            private set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged(() => IsDirty);
                }
            }
        }
        public bool CanBeDirty
        {
            get
            {
                return _canBeDirty;
            }
            set
            {
                if (_canBeDirty != value)
                {
                    _canBeDirty = value;
                    OnPropertyChanged(() => CanBeDirty);
                }
            }
        }

        public bool CanBeDeleted
        {
            get
            {
                return _canBeDeleted;
            }
            set
            {
                if (_canBeDeleted != value)
                {
                    _canBeDeleted = value;
                    OnPropertyChanged(() => CanBeDeleted);
                }
            }
        }

        #region Commands

        public ICommand CommitCommand
        {
            get
            {
                return _commit
                    ?? (_commit = new RelayCommand(OnCommit, () => IsDirty && SwitchedOn));
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return _delete
                    ?? (_delete = new RelayCommand(OnDelete, () => CanBeDeleted && SwitchedOn));
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return _edit
                    ?? (_edit = new RelayCommand(
                                          () =>
                                          {
                                              IsEditorActive = !IsEditorActive;
                                          },
                                          () => SwitchedOn));
            }
        }

        public ICommand RevertCommand
        {
            get
            {
                return _revert
                    ?? (_revert = new RelayCommand(
                                          () =>
                                          {
                                              IsEditorActive = false;
                                          },
                                          () => SwitchedOn));
            }
        }

        #endregion Commands

        public void MarkDirty()
        {
            if (CanBeDirty)
            {
                IsDirty = true;

                var h = ModelPropertyChanged;
                if (h != null)
                {
                    h(this, new EditableEventArgs(vm));
                }
            }
        }

        public void StartTrackDirt()
        {
            CanBeDirty = true;
        }

        #endregion IEditable

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vm">ViewModel to be edited</param>
        /// <param name="switchedOn">Initial state of commands. Default is "off".</param>
        /// <param name="dirtImmunity">Initial state of CanBeDirty. Default is "true".</param>
        public EditableBase(ViewModelBase vm, bool switchedOn = false, bool dirtImmunity = false)
        {
            this.vm = vm;
            SwitchedOn = switchedOn;
            CanBeDirty = !dirtImmunity;
        }
        /// <summary>
        /// ViewModel to be edited inherits from EditableBase.
        /// </summary>
        /// <param name="switchedOn">Initial state of commands. Default is "off".</param>
        /// <param name="dirtImmunity">Initial state of CanBeDirty. Default is "true".</param>
        protected EditableBase(bool switchedOn = false, bool dirtImmunity = false)
        {
            vm = this;
            SwitchedOn = switchedOn;
            CanBeDirty = !dirtImmunity;
        }

        private void OnCommit()
        {
            IsEditorActive = false;
            IsDirty = false;

            var h = Committed;
            if (h != null)
            {
                h(this, new EditableEventArgs(vm));
            }
        }

        private void OnDelete()
        {
            var h = Deleted;
            if (h != null)
            {
                h(this, new EditableEventArgs(vm));
            }
        }
    }
}