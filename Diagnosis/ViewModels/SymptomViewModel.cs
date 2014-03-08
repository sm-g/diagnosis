using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Collections.Generic;

namespace Diagnosis.ViewModels
{
    public class SymptomViewModel : ViewModelBase, ISearchable
    {
        private SymptomViewModel _parent;
        private bool _isChecked;

        private SymptomSearchViewModel _search;

        public string SortingOrder { get; private set; }

        internal Symptom Symptom { get; private set; }

        public string Name
        {
            get
            {
                return Symptom.Title;
            }
            set
            {
                if (Symptom.Title != value)
                {
                    Symptom.Title = value;
                    OnPropertyChanged(() => Name);
                }
            }
        }

        public string Representation
        {
            get
            {
                return Name;
            }
        }

        public SymptomViewModel Parent
        {
            get
            {
                return _parent;
            }
            private set
            {
                _parent = value;
            }
        }

        public ObservableCollection<SymptomViewModel> Children { get; private set; }

        public IEnumerable<SymptomViewModel> AllChildren
        {
            get
            {
                List<SymptomViewModel> result = Children.ToList();
                foreach (var child in Children)
                {
                    result.AddRange(child.AllChildren);
                }
                return result;
            }
        }

        public ObservableCollection<SymptomViewModel> NonTerminalChildren
        {
            get
            {
                return new ObservableCollection<SymptomViewModel>(Children.Where(i => !i.IsTerminal));
            }
        }

        public ObservableCollection<SymptomViewModel> TerminalChildren
        {
            get
            {
                return new ObservableCollection<SymptomViewModel>(Children.Where(i => i.IsTerminal));
            }
        }

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (_isChecked != value)
                {
                    if (IsGroup)
                    {
                        _isChecked = false;
                    }
                    else
                    {
                        _isChecked = value;
                    }

                    OnPropertyChanged(() => IsChecked);
                    if (!IsGroup)
                    {
                        PropagateCheckedState(value);
                        BubbleCheckedChildren();
                    }
                    this.Send((int)EventID.SymptomCheckedChanged, new SymptomCheckedChangedParams(this, IsChecked).Params);
                }
            }
        }

        public bool IsGroup
        {
            get
            {
                return Symptom.IsGroup;
            }
            set
            {
                if (Symptom.IsGroup != value)
                {
                    Symptom.IsGroup = value;
                    IsChecked = !value; // группа не может быть отмечена

                    OnPropertyChanged(() => IsGroup);
                }
            }
        }

        public bool IsTerminal
        {
            get
            {
                return Children.Count == 0;
            }
        }

        public bool IsRoot
        {
            get
            {
                return Parent == null;
            }
        }

        public int CheckedChildren
        {
            get
            {
                if (IsTerminal)
                    return 0;
                return Children.Sum(s => s.CheckedChildren + (s.IsChecked ? 1 : 0));
            }
        }

        public SymptomSearchViewModel Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new SymptomSearchViewModel(this);
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }
        public SymptomViewModel(Symptom s)
        {
            Contract.Requires(s != null);
            Symptom = s;
            Children = new ObservableCollection<SymptomViewModel>();
        }

        public SymptomViewModel(string title)
            : this(new Symptom() { Title = title })
        {
        }

        internal SymptomViewModel()
            : this(new Symptom())
        {
        }

        public void ToggleChecked()
        {
            IsChecked = !IsChecked;
        }

        public SymptomViewModel Add(Symptom symptom)
        {
            return Add(new SymptomViewModel(symptom));
        }

        public SymptomViewModel Add(SymptomViewModel symptomVM)
        {
            Contract.Requires(symptomVM != null);

            symptomVM.Parent = this;
            Children.Add(symptomVM);
            OnChildAdded();
            return this;
        }

        public SymptomViewModel Remove(SymptomViewModel symptomVM)
        {
            Children.Remove(symptomVM);
            OnChildRemoved();
            return this;
        }

        public void Delete()
        {
            if (!IsRoot)
            {
                IsChecked = false;
                Parent.Remove(this);
            }
        }

        public void CheckChild(SymptomViewModel symptom, bool inAllChildren)
        {
            if (symptom != null)
            {
                var query = inAllChildren ? AllChildren : Children;

                if (query.SingleOrDefault(child => child == symptom) == null)
                    Add(symptom);

                symptom.IsChecked = true;
            }
        }

        private void PropagateCheckedState(bool newState)
        {
            if (newState && !IsRoot)
            {
                Parent.IsChecked = true;
            }

            if (!newState)
            {
                foreach (var item in Children)
                {
                    item.IsChecked = false;
                }
            }
        }

        private void BubbleCheckedChildren()
        {
            OnPropertyChanged(() => CheckedChildren);
            if (!IsRoot)
            {
                Parent.BubbleCheckedChildren();
            }
        }

        private void OnChildAdded()
        {
            OnPropertyChanged(() => TerminalChildren);
            OnPropertyChanged(() => IsTerminal);
        }

        private void OnChildRemoved()
        {
            OnPropertyChanged(() => TerminalChildren);
            OnPropertyChanged(() => NonTerminalChildren);
            OnPropertyChanged(() => IsTerminal);
        }

        void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            CheckChild(Search.SelectedItem, Search.AllChildren);
            Search.Clear();
        }

        internal void Initialize()
        {
            int i = 1;
            foreach (SymptomViewModel child in Children)
            {
                child.Parent = this;
                child.SortingOrder = this.SortingOrder + i++;
                child.Initialize();
            }
        }
    }
}