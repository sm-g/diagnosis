﻿using Diagnosis.Models;
using EventAggregator;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using Diagnosis.Core;

namespace Diagnosis.ViewModels
{
    public class WordViewModel : HierarchicalBase<WordViewModel>
    {
        internal readonly Word word;

        private Category _defCat;
        private ICommand _sendToSearch;
        private PopupSearch<WordViewModel> _search;

        public Editable Editable { get; private set; }

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
                return _defCat;
            }
            set
            {
                if (_defCat != value)
                {
                    if (value != null)
                        word.DefaultCategory = value;
                    _defCat = value;

                    OnPropertyChanged("DefaultCategory");
                }
            }
        }

        #endregion

        public ICommand SendToSearchCommand
        {
            get
            {
                return _sendToSearch
                   ?? (_sendToSearch = new RelayCommand(() =>
                   {
                       this.Send(Events.SendToSearch, this.ToEnumerable().AsParams(MessageKeys.Words));
                   }));
            }
        }

        public string SearchText
        {
            get
            {
                return Name;
            }
        }

        public PopupSearch<WordViewModel> Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new PopupSearch<WordViewModel>(new WordSearcher(this, new HierarchicalSearchSettings()));
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

        public void RefreshProperties()
        {
            OnPropertyChanged("Name");
            OnPropertyChanged("Priority");
            OnPropertyChanged("Unsaved");
        }

        public WordViewModel(Word w)
        {
            Contract.Requires(w != null);
            word = w;


            DefaultCategory = w.DefaultCategory;

            Editable = new Editable(word);

            this.ParentChanged += WordViewModel_ParentChanged;
        }

        private void WordViewModel_ParentChanged(object sender, HierarchicalEventAgrs<WordViewModel> e)
        {
            // this - родитель
            // добавили слово не в корень — устанавливаем родителя слову
            if (e.IHierarchical.word.Parent == null && !IsRoot)
            {
                e.IHierarchical.word.Parent = this.word;
            }
        }

        private void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            this.AddIfNotExists(Search.SelectedItem);
            Search.SelectedItem.IsChecked = true;
            Search.Clear();
        }

        public override string ToString()
        {
            return word.ToString();
        }
    }
}