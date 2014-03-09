using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Collections.Generic;

namespace Diagnosis.ViewModels
{
    public class SymptomViewModel : HierarchicalBaseViewModel<SymptomViewModel>, ISearchable, ICheckableHierarchical<SymptomViewModel>
    {
        private bool _isChecked;
        Symptom symptom;

        private SymptomSearch _search;

        public string SortingOrder { get; private set; }


        public int Level
        {
            get
            {
                return symptom.Level;
            }
            set
            {
                if (symptom.Level != value)
                {
                    symptom.Level = value;
                    OnPropertyChanged(() => Level);
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

        #region IHierarchical

        public override string Name
        {
            get
            {
                return symptom.Title;
            }
            set
            {
                if (symptom.Title != value)
                {
                    symptom.Title = value;
                    OnPropertyChanged(() => Name);
                }
            }
        }

        #endregion

        #region ICheckableHierarchical

        bool _isNonCheckable;
        public bool IsNonCheckable
        {
            get
            {
                return
                    _isNonCheckable;
            }
            set
            {
                if (_isNonCheckable != value)
                {
                    _isNonCheckable = value;
                    IsChecked = !value;

                    OnPropertyChanged(() => IsNonCheckable);
                }
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
                    if (IsNonCheckable)
                    {
                        _isChecked = false;
                    }
                    else
                    {
                        _isChecked = value;
                    }

                    OnPropertyChanged(() => IsChecked);
                    if (!IsNonCheckable)
                    {
                        PropagateCheckedState(value);
                        BubbleCheckedChildren();
                    }

                    this.Send((int)EventID.SymptomCheckedChanged, new SymptomCheckedChangedParams(this, IsChecked).Params);
                }
            }
        }

        public void ToggleChecked()
        {
            IsChecked = !IsChecked;
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

        #endregion


        public SymptomSearch Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new SymptomSearch(this);
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }
        public SymptomViewModel(Symptom s)
        {
            Contract.Requires(s != null);
            symptom = s;
        }

        public SymptomViewModel(string title)
            : this(new Symptom() { Title = title })
        {
        }

        internal SymptomViewModel()
            : this(new Symptom())
        {
        }
        void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            this.AddIfNotExists(Search.SelectedItem, Search.AllChildren);
            Search.SelectedItem.IsChecked = true;
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