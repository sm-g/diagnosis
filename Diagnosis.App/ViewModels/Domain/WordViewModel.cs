using Diagnosis.Models;
using EventAggregator;
using Diagnosis.App.Messaging;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public class WordViewModel : HierarchicalCheckable<WordViewModel>
    {
        internal readonly Word word;

        private CategoryViewModel _defCat;
        private PopupSearch<WordViewModel> _search;

        public Editable Editable { get; private set; }

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
                    Editable.MarkDirty();
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
                    Editable.MarkDirty();
                }
            }
        }
        public bool IsEnum
        {
            get
            {
                return word.IsEnum;
            }
            set
            {
                if (word.IsEnum != value)
                {
                    word.IsEnum = value;
                    OnPropertyChanged("IsEnum");
                    Editable.MarkDirty();

                }
            }
        }
        public CategoryViewModel DefaultCategory
        {
            get
            {
                return _defCat;
            }
            set
            {
                if (_defCat != value)
                {
                    if (value != null)
                        word.DefaultCategory = value.category;
                    _defCat = value;

                    OnPropertyChanged("DefaultCategory");
                    Editable.MarkDirty();
                }
            }
        }

        public string SearchText
        {
            get
            {
                return Name;
            }
        }

        public override void OnCheckedChanged()
        {
            base.OnCheckedChanged();

            this.Send((int)EventID.WordCheckedChanged, new WordParams(this).Params);
        }

        public PopupSearch<WordViewModel> Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new PopupSearch<WordViewModel>(new WordSearcher(this, new SimpleSearcherSettings() { AllChildren = true }));
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }

        public bool Unsaved
        {
            get
            {
                return word.Id == 0;
            }
        }

        public WordViewModel(Word w)
        {
            Contract.Requires(w != null);
            word = w;

            Editable = new Editable(this, dirtImmunity: true);

            DefaultCategory = EntityManagers.CategoryManager.GetByModel(w.DefaultCategory);

            Editable.CanBeDirty = true;
        }

        private void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            this.AddIfNotExists(Search.SelectedItem, Search.searcher.AllChildren);
            Search.SelectedItem.IsChecked = true;
            Search.Clear();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}