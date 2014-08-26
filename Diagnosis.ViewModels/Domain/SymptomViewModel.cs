using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
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


            Words = new ObservableCollection<WordViewModel>(EntityProducers.WordsProducer.GetSymptomWords(s));
            DefaultCategory = symptom.DefaultCategory;

            Editable = new Editable(symptom);
        }


        public override string ToString()
        {
            return symptom.ToString();
        }
    }
}