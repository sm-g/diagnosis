using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using log4net;
using NHibernate.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace Diagnosis.ViewModels.Autocomplete
{
    public class MeasureEditorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MeasureEditorViewModel));

        private List<Uom> _uoms;
        private string _val;
        private bool isValueValid;
        MeasureEditorViewModel(Measure measure, Word w)
        {
            _uoms = new List<Uom> { Uom.Null };
            var allUoms = UomQuery.Contains(Session)("");
            _uoms.AddRange(allUoms);

            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(_uoms);
            view.GroupDescriptions.Add(new PropertyGroupDescription("Type"));


            if (measure == null)
            {
                Measure = new Measure(0) { Word = w };
            }
            else
            {
                Measure = new Measure(measure.Value, measure.Uom)
                {
                    Word = w ?? measure.Word  // новое слово или бывшее с измерением
                };
            }
            Value = Measure.Value.ToString();

            Autocomplete = new AutocompleteViewModel(
                new Recognizer(Session)
                {
                    OnlyWords = true,
                    AddQueryToSuggestions = true,
                    CanChangeAddQueryToSuggstions = false
                },
                AutocompleteViewModel.OptionsMode.MeasureEditor,
                Word == null ? null : new[] { new ConfindenceHrItemObject(Word, Confidence.Present) })
                {
                    IsDragSourceEnabled = false,
                    IsDropTargetEnabled = false
                };
            Autocomplete.EntitiesChanged += (s, e) =>
            {
                var wordHio = Autocomplete.GetCHIOs().FirstOrDefault();
                Word = wordHio != null ? wordHio.HIO as Word : null;
            };

            Title = "Редактирование измерения";
            HelpTopic = "editmeasure";
            WithHelpButton = false;
        }
        /// <summary>
        /// Edit
        /// </summary>
        /// <param name="measure"></param>
        public MeasureEditorViewModel(Measure measure)
            : this(measure, null)
        {

        }
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="word"></param>
        public MeasureEditorViewModel(Word word)
            : this(null, word)
        {
        }
        /// <summary>
        /// Create
        /// </summary>
        public MeasureEditorViewModel()
            : this(null, null)
        {
        }

        public AutocompleteViewModel Autocomplete { get; private set; }

        public string Value
        {
            get { return _val; }
            set { _val = value; }
        }

        public Uom Uom
        {
            get { return Measure.Uom; }
            set { Measure.Uom = value; }
        }

        public Word Word
        {
            get { return Measure.Word; }
            set { Measure.Word = value; }
        }
        public Measure Measure { get; private set; }

        public IEnumerable<Uom> Uoms { get { return _uoms; } }

        public override bool CanOk
        {
            get
            {
                return Word != null && Measure != null && isValueValid;
            }
        }

        public override string this[string columnName]
        {
            get
            {
                double d;
                if (columnName == "Value")
                    isValueValid = double.TryParse(Value, out d);

                if (!isValueValid)
                    return "Из этого не получается число.";
                return string.Empty;
            }
        }

        protected override void OnOk()
        {
            Measure.Value = double.Parse(Value);
        }

        protected override void OnCancel()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}