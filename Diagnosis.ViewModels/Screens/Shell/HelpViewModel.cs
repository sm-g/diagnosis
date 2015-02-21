using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Screens
{
    public class HelpViewModel : WindowViewModel
    {
        private string _topic;

        public HelpViewModel()
        {
            Title = "Помощь";
        }

        public HelpViewModel(string topic)
            : this()
        {
            Topic = topic;
        }

        public string Topic
        {
            get
            {
                return _topic;
            }
            set
            {
                if (_topic != value)
                {
                    _topic = value;
                    OnPropertyChanged(() => Topic);
                }
            }
        }
    }
}
