using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Screens
{
    public abstract class ScreenBase : SessionVMBase
    {
        private string _title;

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
    }
}
