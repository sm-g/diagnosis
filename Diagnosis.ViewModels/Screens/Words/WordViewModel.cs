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

        public HrCategory DefaultCategory
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