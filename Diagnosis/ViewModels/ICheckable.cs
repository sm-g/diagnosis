﻿using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public interface ICheckable
    {
        bool IsChecked { get; set; }

        bool IsNonCheckable { get; }

        ICommand ToggleCommand { get; }
    }
}