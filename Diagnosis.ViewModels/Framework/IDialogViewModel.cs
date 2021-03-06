﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public interface IDialogViewModel : INotifyPropertyChanged
    {
        bool? DialogResult { get; }
        string Title { get; }
        string HelpTopic { get; }
        bool WithHelpButton { get; }
        void OnDialogResult(Action trueAct, Action falseAct = null);
        void OnDialogResult(Action<bool> act);
        ICommand OkCommand { get; }
        ICommand ApplyCommand { get; }
        ICommand CancelCommand { get; }
        bool CanOk { get; }
        bool CanApply { get; }
    }
}
