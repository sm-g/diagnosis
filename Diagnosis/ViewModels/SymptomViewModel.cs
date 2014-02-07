using Diagnosis.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using EventAggregator;

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
                if (_isChecked != value)
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

        public static List<SymptomViewModel> CreateSymptoms()
        {
            SymptomViewModel root1 = new SymptomViewModel(new Symptom()
            {
                Title = "голова",
                IsGroup = true
            })
            {
                Children =
                {
                    new SymptomViewModel(new Symptom()
                    {
                        Title = "нос"
                    }),
                    new SymptomViewModel(new Symptom()
                    {
                        Title = "больные зубы"
                    })
                    {
                        Children = {
                            new SymptomViewModel(new Symptom()
                            {
                                Title = "боль в зубе"
                            }),
                            new SymptomViewModel(new Symptom()
                            {
                                Title = "шатается зуб"
                            })
                        }
                    },
                    new SymptomViewModel(new Symptom()
                    {
                        Title = "уши"
                    }),
                }
            };
            SymptomViewModel root2 = new SymptomViewModel(new Symptom()
            {
                Title = "ноги",
                IsGroup = true

            });

            root1.Initialize();
            root2.Initialize();

            return new List<SymptomViewModel>() { root1, root2 };
        }

        public void ToggleChecked()
        {
            IsChecked = !IsChecked;
        }

        void VerifyTreeState()
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

        private void Initialize()
        {
            foreach (SymptomViewModel child in Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }
    }
}