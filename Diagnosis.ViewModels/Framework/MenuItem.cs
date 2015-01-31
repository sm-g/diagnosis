using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    class MenuItem : ViewModelBase
    {
        private string _text;
        bool _visible;
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged(() => Text);
                }
            }
        }
        public string InputGestureText { get; set; }
        public ObservableCollection<MenuItem> Children { get; private set; }
        public ICommand Command { get; set; }
        public bool IsVisible
        {
            get
            {
                return _visible;
            }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnPropertyChanged(() => IsVisible);
                }
            }
        }
        public MenuItem(string item, ICommand command)
        {
            Text = item;
            Command = command;
            IsVisible = true;
            Children = new ObservableCollection<MenuItem>();
        }
    }
}
