using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class SymptomViewModel : CheckableBase
    {
        internal readonly Symptom symptom;
        private Category _defCat;

        private IcdDisease _disease;

        public Editable Editable { get; private set; }

        public string Name
        {
            get
            {
                return (Disease != null ? Disease.Code + ". " : "") +
                    string.Join(" ", Words.OrderBy(w => w.Priority).Select(w => w.Name));
            }
        }

        #region Model

        public IEnumerable<WordViewModel> Words { get; private set; }
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
                    OnPropertyChanged("Disease");
                    Editable.MarkDirty();
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
                        symptom.DefaultCategory = value;
                    _defCat = value;

                    OnPropertyChanged("DefaultCategory");
                    Editable.MarkDirty();
                }
            }
        }

        public bool IsDiagnosis
        {
            get
            {
                return symptom.IsDiagnosis;
            }
            set
            {
                if (symptom.IsDiagnosis != value)
                {
                    symptom.IsDiagnosis = value;
                    OnPropertyChanged("IsDiagnosis");
                    Editable.MarkDirty();
                }
            }
        }

        #endregion

        public string SearchText
        {
            get
            {
                return Name;
            }
        }
        public bool Unsaved
        {
            get
            {
                return symptom.Id == 0;
            }
        }

        public SymptomViewModel(Symptom s)
        {
            Contract.Requires(s != null);
            symptom = s;

            Editable = new Editable(symptom, dirtImmunity: true);

            Words = new ObservableCollection<WordViewModel>(EntityProducers.WordsProducer.GetSymptomWords(s));
            DefaultCategory = symptom.DefaultCategory;

            Editable.CanBeDirty = true;
        }


        public override string ToString()
        {
            return symptom.ToString();
        }
    }
}