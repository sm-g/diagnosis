using Diagnosis.Models;
using NHibernate;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels
{
    public class WordEditorViewModel : ViewModelBase
    {
        private Word _word;
        private readonly ISession session;

        public WordEditorViewModel(ISession session)
        {
            this.session = session;
        }

        public Word Word
        {
            get
            {
                return _word;
            }
            set
            {
                if (_word != value)
                {
                    _word = value;
                    OnPropertyChanged("Word");
                }
            }
        }

        public string Title
        {
            get
            {
                return _word.Title;
            }
            set
            {
                _word.Title = value;
            }
        }
    }
}