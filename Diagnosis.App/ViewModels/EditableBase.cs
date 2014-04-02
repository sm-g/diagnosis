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
        bool _isDirty;
        private bool _switchedOn;
        private bool _deletable;

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

        public bool CanBeDeleted
        {
            get
            {
                return _deletable;
            }
            set
            {
                if (_deletable != value)
                {
                    _deletable = value;
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
            IsDirty = true;

            var h = ModelPropertyChanged;
            if (h != null)
            {
                h(this, new EditableEventArgs(vm));
            }
        }

        #endregion IEditable

        public EditableBase(ViewModelBase vm, bool switchedOn = false)
        {
            this.vm = vm;
            SwitchedOn = switchedOn;
        }

        public EditableBase(bool switchedOn = false)
        {
            vm = this; // if vm inherited from EditableBase
            SwitchedOn = switchedOn;
        }

        private void OnCommit()
        {
            IsEditorActive = false;

            var h = Committed;
            if (h != null)
            {
                h(this, new EditableEventArgs(vm));
            }

            IsDirty = false;
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