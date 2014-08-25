using System;
using System.Diagnostics;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using Diagnosis.Models;

namespace Diagnosis.ViewModels
{
    /// <summary>
    /// Позволяет редактировать сущность. Содержит команды для открытия редактора, сохранения и отмены изменений, удаления сущности. Хранит чистоту сущности.
    /// </summary>
    public class Editable : ViewModelBase
    {
        #region Fields

        private readonly IEntity entity;

        private ICommand _commit;
        private ICommand _delete;
        private ICommand _edit;
        private ICommand _revert;
        private bool _editorActive;
        private bool _editorFocused;
        private bool _canBeDirty;
        private bool _isDirty;
        private bool _switchedOn;
        private bool _canBeDeleted;

        #endregion

        public event EditableEventHandler Committed;

        public event EditableEventHandler Reverted;

        public event EditableEventHandler Deleted;

        public event EditableEventHandler DirtyChanged;

        public bool IsEditorActive
        {
            get
            {
                return _editorActive;
            }
            set
            {
                if (_editorActive != value)
                {
                    if (value && entity is IEditableObject)
                    {
                        (entity as IEditableObject).BeginEdit();
                    }
                    _editorActive = value;
                    Debug.Print("editor {0} active = {1}", entity, value);
                    OnPropertyChanged("IsEditorActive");
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
                    Debug.Print("editor {0} focused = {1}", entity, value);
                    _editorFocused = value;
                    OnPropertyChanged("IsEditorFocused");
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
                    OnPropertyChanged("SwitchedOn");
                }
            }
        }

        /// <summary>
        /// Показывает, что есть несохраненные изенения, разрешает выполнять сохранение.
        /// </summary>
        public bool IsDirty
        {
            get { return _isDirty; }
            internal set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;

                    if (value)
                        WasDirty = true;

                    var h = DirtyChanged;
                    if (h != null)
                    {
                        h(this, new EditableEventArgs(entity));
                    }

                    Debug.Print("isdirty {0} = {1}", entity, value);
                    OnPropertyChanged("IsDirty");
                    OnPropertyChanged("WasDirty");
                }
            }
        }

        public bool WasDirty { get; private set; }

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
                    OnPropertyChanged("CanBeDeleted");
                }
            }
        }

        #region Commands
        /// <summary>
        /// Открывает и закрывает редактор.
        /// </summary>
        public ICommand EditCommand
        {
            get
            {
                return _edit
                    ?? (_edit = new RelayCommand(ToggleEditor, () => SwitchedOn));
            }
        }

        /// <summary>
        /// Сохраняет изменения и закрывает редактор.
        /// </summary>
        public ICommand CommitCommand
        {
            get
            {
                return _commit
                    ?? (_commit = new RelayCommand(OnCommit, () => IsDirty && SwitchedOn));
            }
        }
        /// <summary>
        /// Отменяет изменения и закрывает редактор.
        /// </summary>
        public ICommand RevertCommand
        {
            get
            {
                return _revert
                    ?? (_revert = new RelayCommand(OnRevert, () => IsDirty && IsEditorActive && SwitchedOn));
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
        /// <summary>
        /// Открывает и закрывает редактор.
        /// </summary>
        public void ToggleEditor()
        {
            IsEditorActive = !IsEditorActive;
        }

        /// <summary>
        /// Сохраняет изменения и закрывает редактор. Возвращает true, если было сохранение.
        /// </summary>
        /// <param name="force">Принудительное сохранение игнорируя IsDirty.</param>
        public bool Commit(bool force = false)
        {
            if ((IsDirty || force) && SwitchedOn)
            {
                OnCommit();
                return true;
            }
            return false;
        }

        public bool Delete()
        {
            if (CanBeDeleted && SwitchedOn)
            {
                OnDelete();
                return true;
            }
            return false;
        }

        #endregion Commands

        internal void MarkDirty()
        {
            IsDirty = true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="entity">Model to be edited</param>
        /// <param name="switchedOn">Initial state of commands. Default is "off".</param>
        /// <param name="deletable">Initial state of CanBeDeleted. Default is "true".</param>
        public Editable(IEntity entity, bool switchedOn = false, bool deletable = true)
        {
            this.entity = entity;
            SwitchedOn = switchedOn;
            CanBeDeleted = deletable;
        }

        private void OnCommit()
        {
            Debug.Print("committing {0}", entity);

            if (entity is IEditableObject)
            {
                (entity as IEditableObject).EndEdit();
            }

            var h = Committed;
            if (h != null)
            {
                h(this, new EditableEventArgs(entity));
                Debug.Print("committed {0}", entity);
            }

            IsEditorActive = false;
            IsDirty = false;
        }

        private void OnRevert()
        {
            Debug.Print("reverting {0}", entity);

            if (entity is IEditableObject)
            {
                (entity as IEditableObject).CancelEdit();
            }

            var h = Reverted;
            if (h != null)
            {
                h(this, new EditableEventArgs(entity));
                Debug.Print("reverted {0}", entity);
            }

            IsEditorActive = false;
            IsDirty = false;
        }

        private void OnDelete()
        {
            Debug.Print("deleting {0}", entity);

            var h = Deleted;
            if (h != null)
            {
                h(this, new EditableEventArgs(entity));
                Debug.Print("deleted {0}", entity);
            }
        }
    }

    public delegate void EditableEventHandler(object sender, EditableEventArgs e);

    public class EditableEventArgs : EventArgs
    {
        public IEntity entity;

        [DebuggerStepThrough]
        public EditableEventArgs(IEntity entity)
        {
            this.entity = entity;
        }
    }
}