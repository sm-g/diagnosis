using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class DialogViewModel : SessionVMBase, IDialog
    {
        private ICommand _okCommand;
        private ICommand _applyCommand;
        private ICommand _cancelCommand;
        private bool _canOk;
        private bool _canApply;
        private bool? _dialogResult;
        private string _title;

        public DialogViewModel()
        {
            CanOk = true;
        }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(() => Title);
                }
            }
        }

        public ICommand OkCommand
        {
            get
            {
                return _okCommand
                   ?? (_okCommand = new RelayCommand(() =>
                   {
                       OnOk();
                       DialogResult = true;
                   }, () => CanOk));
            }
        }

        public virtual bool CanOk
        {
            get
            {
                return _canOk;
            }
            set
            {
                if (_canOk != value)
                {
                    _canOk = value;
                    OnPropertyChanged(() => CanOk);
                }
            }
        }

        public ICommand ApplyCommand
        {
            get
            {
                return _applyCommand
                   ?? (_applyCommand = new RelayCommand(() =>
                   {
                       OnApply();
                   }, () => CanApply));
            }
        }

        public virtual bool CanApply
        {
            get
            {
                return _canApply;
            }
            set
            {
                if (_canApply != value)
                {
                    _canApply = value;
                    OnPropertyChanged(() => CanApply);
                }
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand
                   ?? (_cancelCommand = new RelayCommand(() =>
                   {
                       OnCancel();
                       DialogResult = false;
                   })); // всегда можно отменить
            }
        }
        public bool? DialogResult
        {
            get
            {
                return _dialogResult;
            }
            set
            {
                if (_dialogResult != value)
                {
                    _dialogResult = value;
                    OnPropertyChanged(() => DialogResult);
                }
            }
        }

        protected virtual void OnOk() { }
        protected virtual void OnCancel() { }
        protected virtual void OnApply() { }
    }
}
