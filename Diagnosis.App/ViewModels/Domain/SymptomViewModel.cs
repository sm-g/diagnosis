using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class SymptomViewModel : CheckableBase
    {
        internal readonly Symptom symptom;

        public EditableBase Editable { get; private set; }

        public string SortingOrder { get; private set; }

        public string Name
        {
            get
            {
                return (Disease != null ? Disease.Code + ". " : "") +
                    string.Concat(Words.OrderBy(w => w.Priority).Select(w => w.Name + " "));
            }
        }

        public ObservableCollection<WordViewModel> Words { get; private set; }


        private CategoryViewModel _defCat;
        private IcdDisease _disease;
        public IcdDisease Disease
        {
            get
            {
                return _disease;
            }
            set
            {
                if (_disease != value)
                {
                    _disease = value;
                    OnPropertyChanged(() => Disease);
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
                        symptom.DefaultCategory = value.category;
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
        }

        public SymptomViewModel(Symptom s)
        {
            Contract.Requires(s != null);

            symptom = s;
            Editable = new EditableBase(this);

            Words = new ObservableCollection<WordViewModel>(EntityManagers.WordsManager.GetSymptomWords(s));
            DefaultCategory = EntityManagers.CategoryManager.GetByModel(symptom.DefaultCategory);
        }


        public override string ToString()
        {
            return Name;
        }
    }
}