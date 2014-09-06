using System;

namespace Diagnosis.ViewModels
{
    public class PanelViewModel : ViewModelBase
    {
        private ViewModelBase _content;
        private bool _opened;
        private string _title;
        private DateTime openedChangedAt;

        public PanelViewModel(ViewModelBase content)
        {
            Content = content;
        }

        public ViewModelBase Content
        {
            get
            {
                return _content;
            }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged(() => Content);
                }
            }
        }

        public bool Opened
        {
            get
            {
                return _opened;
            }
            set
            {
                if (_opened != value && NotReOpenedFix())
                {
                    _opened = value;
                    openedChangedAt = DateTime.UtcNow;
                    OnPropertyChanged(() => Opened);
                }
            }
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

        public RelayCommand ToggleCommand
        {
            get
            {
                return new RelayCommand(() =>
                       {
                           Opened = !Opened;
                       });
            }
        }

        private bool NotReOpenedFix()
        {
            // Если открываем aside по togglebutton, а закрываем через меню, aside открывается повторно
            return (DateTime.UtcNow - openedChangedAt).TotalMilliseconds > 100;
        }
    }
}