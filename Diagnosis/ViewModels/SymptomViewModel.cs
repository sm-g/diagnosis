using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class SymptomViewModel : ViewModelBase
    {
        private SymptomViewModel _parent;
        private bool _isChecked;

        public string Order { get; private set; }

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
                    _isChecked = value;
                    OnPropertyChanged(() => IsChecked);
                    if (!IsGroup)
                    {
                        PropagateTreeState(value);
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
                    IsChecked = !value;

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

        public SymptomViewModel(Symptom s)
        {
            Contract.Requires(s != null);
            Symptom = s;
            Children = new ObservableCollection<SymptomViewModel>();
        }

        public SymptomViewModel(string title)
        {
            Contract.Requires(title != null);
            Contract.Requires(title.Length > 0);

            Symptom = new Symptom() { Title = title };
            Children = new ObservableCollection<SymptomViewModel>();
        }

        public void ToggleChecked()
        {
            IsChecked = !IsChecked;
        }

        private void PropagateTreeState(bool newState)
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

        protected void BubbleCheckedChildren()
        {
            OnPropertyChanged(() => CheckedChildren);
            if (!IsRoot)
                Parent.BubbleCheckedChildren();
        }

        internal void Initialize()
        {
            int i = 1;
            foreach (SymptomViewModel child in Children)
            {
                child.Parent = this;
                child.Order = this.Order + i++;
                child.Initialize();
            }
        }
    }
}