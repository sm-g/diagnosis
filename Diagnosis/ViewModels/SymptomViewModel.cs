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

        public Symptom Symptom { get; private set; }

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
        }

        public ObservableCollection<SymptomViewModel> Children { get; private set; }

        public ObservableCollection<SymptomViewModel> GroupChildren
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
                if (_isChecked != value && !IsGroup)
                {
                    _isChecked = value;
                    OnPropertyChanged(() => IsChecked);
                    OnPropertyChanged(() => CheckedChildren);
                    this.Send((int)EventID.SymptomCheckedChanged, new SymptomCheckedChangedParams(this, IsChecked).Params);
                    VerifyTreeState();
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

        private void VerifyTreeState()
        {
            if (Parent != null && IsChecked)
            {
                Parent.IsChecked = true;
            }

            if (!IsChecked)
            {
                foreach (var item in Children)
                {
                    item.IsChecked = false;
                }
            }
        }

        internal void Initialize()
        {
            foreach (SymptomViewModel child in Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }
    }
}