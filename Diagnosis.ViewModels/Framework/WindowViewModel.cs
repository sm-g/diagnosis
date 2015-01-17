using System;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class WindowViewModel : SessionVMBase, IWindowViewModel
    {
        private string _title;

        private bool _closed;

        private bool _active;

        public WindowViewModel()
        {
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


        public bool IsClosed
        {
            get
            {
                return _closed;
            }
            set
            {
                if (_closed != value)
                {
                    _closed = value;
                    OnPropertyChanged(() => IsClosed);
                }
            }
        }

        public bool IsActive
        {
            get
            {
                return _active;
            }
            set
            {
                if (_active != value)
                {
                    _active = value;
                    if (value)
                    {
                        // vm reused after window closed
                        IsClosed = false;
                    }
                    OnPropertyChanged(() => IsActive);
                }
            }
        }
    }
}