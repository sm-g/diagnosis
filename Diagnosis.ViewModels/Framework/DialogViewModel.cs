using Diagnosis.ViewModels.Controls;
using Diagnosis.ViewModels.Controls.Autocomplete;
using Diagnosis.ViewModels.Screens;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class DialogViewModel : SessionVMBase, IDialogViewModel
    {
        public readonly static Type[] ChildWindowModalDialogs = new Type[] {
                typeof(AppointmentEditorViewModel),
                typeof(CourseEditorViewModel),
                typeof(AdminSettingsViewModel),
                typeof(SettingsViewModel),
                typeof(MeasureEditorViewModel),
                typeof(DoctorEditorViewModel),
                typeof(WordEditorViewModel),
        };

        private ICommand _okCommand;
        private ICommand _applyCommand;
        private ICommand _cancelCommand;
        private bool _canOk;
        private bool _canApply;
        private bool? _dialogResult;
        private string _title;
        private bool _withHelpButton;

        public DialogViewModel()
        {
            _canOk = true;
            ResizeMode = System.Windows.ResizeMode.NoResize;
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

        public string HelpTopic
        {
            get;
            set;
        }

        public ResizeMode ResizeMode { get; set; }

        public bool WithHelpButton
        {
            get
            {
                return _withHelpButton;
            }
            set
            {
                if (_withHelpButton != value)
                {
                    _withHelpButton = value;
                    OnPropertyChanged(() => WithHelpButton);
                }
            }
        }

        public void OnDialogResult(Action trueAct, Action falseAct = null)
        {
            System.ComponentModel.PropertyChangedEventHandler f = null;
            f = (s, e) =>
            {
                if (e.PropertyName == "DialogResult")
                {
                    var dialog = s as IDialogViewModel;
                    Contract.Assert(dialog.DialogResult.HasValue);
                    dialog.PropertyChanged -= f;
                    if (dialog.DialogResult.Value)
                    {
                        if (trueAct != null)
                            trueAct();
                    }
                    else if (falseAct != null)
                    {
                        falseAct();
                    }
                }
            };
            this.PropertyChanged += f;
        }

        public void OnDialogResult(Action<bool> act)
        {
            System.ComponentModel.PropertyChangedEventHandler f = null;
            f = (s, e) =>
            {
                if (e.PropertyName == "DialogResult")
                {
                    var dialog = s as IDialogViewModel;
                    Contract.Assert(dialog.DialogResult.HasValue);
                    dialog.PropertyChanged -= f;
                    act(dialog.DialogResult.Value);
                }
            };
            this.PropertyChanged += f;
        }

        protected virtual void OnOk() { }
        protected virtual void OnCancel() { }
        protected virtual void OnApply() { }
    }
}
