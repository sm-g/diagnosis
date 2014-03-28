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
        private bool _editActive;
        private bool _editorFocused;
        private ICommand _revert;
        private bool _switchedOn;

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
                if (_editActive != value && (IsReady || !value))
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

        /// <summary>
        /// Показывает, что элемент в состоянии готовности (никаких действий над ним).
        /// </summary>
        public virtual bool IsReady
        {
            get { return !IsEditorActive; }
        }

        public bool IsDirty
        {
            get;
            set;
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
                    ?? (_delete = new RelayCommand(OnDelete, () => SwitchedOn));
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

        #endregion IEditable

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