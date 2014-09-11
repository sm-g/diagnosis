using Diagnosis.Models;
using EventAggregator;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using Diagnosis.Core;

namespace Diagnosis.ViewModels
{
    public class WordViewModel : HierarchicalBase<WordViewModel>
    {
        internal readonly Word word;

        #region Model

        public byte Priority
        {
            get
            {
                return word.Priority;
            }
            set
            {
                if (word.Priority != value)
                {
                    word.Priority = value;
                    OnPropertyChanged("Priority");
                }
            }
        }

        public string Name
        {
            get
            {
                return word.Title;
            }
            set
            {
                if (word.Title != value)
                {
                    word.Title = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public Category DefaultCategory
        {
            get
            {
                return word.DefaultCategory;
            }
            set
            {
                if (word.DefaultCategory != value)
                {
                    word.DefaultCategory = value;

                    OnPropertyChanged("DefaultCategory");
                }
            }
        }

        #endregion

        public ICommand SendToSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                   {
                       this.Send(Events.SendToSearch, word.ToEnumerable().AsParams(MessageKeys.Words));
                   });
            }
        }

        public bool Unsaved
        {
            get
            {
                return word.IsDirty;
            }
        }

        public WordViewModel(Word w)
        {
            Contract.Requires(w != null);
            word = w;


            DefaultCategory = w.DefaultCategory;
        }

        public override string ToString()
        {
            return word.ToString();
        }
    }
}