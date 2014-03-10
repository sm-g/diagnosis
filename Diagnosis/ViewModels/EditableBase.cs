using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public abstract class EditableBase : ViewModelBase, IEditable
    {
        private bool _selected;
        public bool IsSelected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged(() => IsSelected);
                }
            }
        }

        #region IEditable

        private ICommand _commit;
        private ICommand _delete;
        private ICommand _edit;
        private bool _editActive;
        private bool _editorFocused;
        private ICommand _revert;
        public abstract string Name
        {
            get;
            set;
        }

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

        #region Commands

        public ICommand CommitCommand
        {
            get
            {
                return _commit
                    ?? (_commit = new RelayCommand(
                                          () =>
                                          {
                                              IsEditorActive = false;
                                          }));
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return _delete
                    ?? (_delete = new RelayCommand(
                                          () =>
                                          {
                                          }));
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

        #endregion IEditable

    }
}
