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

        private readonly IDomainObject entity;
        private bool _editorActive;
        private bool _canBeDeleted;

        #endregion

        public event EditableEventHandler Committed;

        public event EditableEventHandler Reverted;

        public event EditableEventHandler Deleted;

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
                return new RelayCommand(ToggleEditor);
            }
        }

        /// <summary>
        /// Сохраняет изменения и закрывает редактор.
        /// </summary>
        public ICommand CommitCommand
        {
            get
            {
                return new RelayCommand(OnCommit);
            }
        }
        /// <summary>
        /// Отменяет изменения и закрывает редактор.
        /// </summary>
        public ICommand RevertCommand
        {
            get
            {
                return new RelayCommand(OnRevert);
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(OnDelete, () => CanBeDeleted);
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
        public bool Commit()
        {
            OnCommit();
            return true;
        }

        public bool Delete()
        {
            if (CanBeDeleted)
            {
                OnDelete();
                return true;
            }
            return false;
        }

        #endregion Commands

        /// <summary>
        ///
        /// </summary>
        /// <param name="entity">Model to be edited</param>
        /// 
        public Editable(IDomainObject entity)
        {
            this.entity = entity;
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
        public IDomainObject entity;

        [DebuggerStepThrough]
        public EditableEventArgs(IDomainObject entity)
        {
            this.entity = entity;
        }
    }
}