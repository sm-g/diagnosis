using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class Editable : ViewModelBase
    {
        #region Fields

        private readonly ViewModelBase vm;

        private ICommand _commit;
        private ICommand _delete;
        private ICommand _edit;
        private ICommand _revert;
        private bool _editActive;
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
                return _editActive;
            }
            set
            {
                if (_editActive != value)
                {
                    if (value)
                    {
                        //vm.BeginEdit();
                    }

                    _editActive = value;
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
                    Console.WriteLine("Editor focused = {0}", value);
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
                if (_isDirty != value && CanBeDirty)
                {
                    _isDirty = value;
                    if (value)
                        WasDirty = true;
                    Console.WriteLine("{0} isdirty = {1}", vm, value);
                    var h = DirtyChanged;
                    if (h != null)
                    {
                        h(this, new EditableEventArgs(vm));
                    }
                    OnPropertyChanged("IsDirty");
                    OnPropertyChanged("WasDirty");
                }
            }
        }

        public bool WasDirty { get; private set; }

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
                    OnPropertyChanged("CanBeDirty");
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
                    ?? (_revert = new RelayCommand(OnRevert, () => IsEditorActive && SwitchedOn));
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
        /// Сохраняет изменения и закрывает редактор.
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
        /// <param name="vm">ViewModel to be edited</param>
        /// <param name="switchedOn">Initial state of commands. Default is "off".</param>
        /// <param name="dirtImmunity">Initial state of CanBeDirty. Default is "true" (no immunity).</param>
        /// <param name="deletable">Initial state of CanBeDeleted. Default is "true".</param>
        public Editable(ViewModelBase vm, bool switchedOn = false, bool dirtImmunity = false, bool deletable = true)
        {
            this.vm = vm;
            SwitchedOn = switchedOn;
            CanBeDirty = !dirtImmunity;
            CanBeDeleted = deletable;
        }

        /// <summary>
        /// ViewModel to be edited inherits from Editable.
        /// </summary>
        /// <param name="switchedOn">Initial state of commands. Default is "off".</param>
        /// <param name="dirtImmunity">Initial state of CanBeDirty. Default is "true".</param>
        /// <param name="deletable">Initial state of CanBeDeleted. Default is "true".</param>
        protected Editable(bool switchedOn = false, bool dirtImmunity = false, bool deletable = true)
        {
            vm = this as ViewModelBase;
            SwitchedOn = switchedOn;
            CanBeDirty = !dirtImmunity;
            CanBeDeleted = deletable;
        }
        private void OnCommit()
        {
            // vm.EndEdit();

            var h = Committed;
            System.Console.WriteLine("on committ {0}", vm);
            if (h != null)
            {
                h(this, new EditableEventArgs(vm));
                System.Console.WriteLine("committed {0}", vm);
            }

            IsEditorActive = false;
            IsDirty = false;
        }

        private void OnRevert()
        {
            //vm.CancelEdit();

            var h = Reverted;
            if (h != null)
            {
                h(this, new EditableEventArgs(vm));
            }

            IsEditorActive = false;
            IsDirty = false;
        }

        private void OnDelete()
        {
            var h = Deleted;
            if (h != null)
            {
                h(this, new EditableEventArgs(vm));
                System.Console.WriteLine("deleted {0}", vm);
            }
        }
    }

    public delegate void EditableEventHandler(object sender, EditableEventArgs e);

    public class EditableEventArgs : EventArgs
    {
        public ViewModelBase viewModel;

        [DebuggerStepThrough]
        public EditableEventArgs(ViewModelBase vm)
        {
            viewModel = vm;
        }
    }
}