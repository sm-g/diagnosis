using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

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

        #region IEditable

        public event EventHandler Committed;
        public event EventHandler Deleted;
        public event EventHandler ModelPropertyChanged;

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
                    ?? (_commit = new RelayCommand(OnCommit, () => IsDirty));
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return _delete
                    ?? (_delete = new RelayCommand(OnDelete));
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
                                          }));
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
                                          }));
            }
        }

        #endregion Commands

        public void MarkDirty()
        {
            IsDirty = true;

            var h = ModelPropertyChanged;
            if (h != null)
            {
                h(this, new EventArgs());
            }
        }


        #endregion IEditable

        void OnCommit()
        {
            IsEditorActive = false;

            var h = Committed;
            if (h != null)
            {
                h(this, new EventArgs());
            }

            IsDirty = false;
        }

        void OnDelete()
        {
            var h = Deleted;
            if (h != null)
            {
                h(this, new EventArgs());
            }
        }
    }
}
