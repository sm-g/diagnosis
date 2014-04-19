using Diagnosis.Models;
using EventAggregator;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public class WordViewModel : HierarchicalCheckable<WordViewModel>
    {
        internal readonly Word word;

        private CategoryViewModel _defCat;
        private WordSearch _search;

        public EditableBase Editable { get; private set; }

        public string SortingOrder { get; private set; }

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
                    OnPropertyChanged(() => Priority);
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
                    OnPropertyChanged(() => Name);
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
                    OnPropertyChanged(() => IsEnum);
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

                    OnPropertyChanged(() => DefaultCategory);
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

            this.Send((int)EventID.WordCheckedChanged, new WordCheckedChangedParams(this, IsChecked).Params);
        }

        public WordSearch Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new WordSearch(this);
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }

        public WordViewModel(Word w)
        {
            Contract.Requires(w != null);
            word = w;
            Editable = new EditableBase(this);

            DefaultCategory = EntityManagers.CategoryManager.GetByModel(w.DefaultCategory);
        }

        public WordViewModel(string title)
            : this(new Word(title))
        {
        }

        private void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            this.AddIfNotExists(Search.SelectedItem, Search.AllChildren);
            Search.SelectedItem.checkable.IsChecked = true;
            Search.Clear();
        }

        internal void Initialize()
        {
            int i = 1;
            foreach (WordViewModel child in Children)
            {
                child.Parent = this;
                child.SortingOrder = this.SortingOrder + i++;
                child.Initialize();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}