﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public interface IDialog
    {
        bool? DialogResult { get; }
        string Title { get; set; }
        ICommand OkCommand { get; }
        ICommand ApplyCommand { get; }
        ICommand CancelCommand { get; }
        bool CanOk { get; }
        bool CanApply { get; }
    }
}
