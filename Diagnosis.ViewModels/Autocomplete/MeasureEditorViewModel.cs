using Diagnosis.Data.Queries;
using Diagnosis.Models;
using log4net;
using System;
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
        private MeasureOperator _op;
        private bool isValueValid;
        private readonly bool withCompare;

        private MeasureEditorViewModel(Measure measure, Word w, bool withCompare)
        {
            this.withCompare = withCompare;

            _uoms = new List<Uom> { Uom.Null };
            var allUoms = UomQuery.Contains(Session)("");
            _uoms.AddRange(allUoms);

            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(_uoms);
            view.GroupDescriptions.Add(new PropertyGroupDescription("Type"));


            if (measure == null)
            {
                Measure = WithCompare ? new MeasureOp(MeasureOperator.Equal, 0) : new Measure(0);
                Measure.Word = w;
            }
            else
            {
                Measure = WithCompare ? new MeasureOp(MeasureOperator.Equal, measure.Value, measure.Uom) : new Measure(measure.Value, measure.Uom);
                Measure.Word = w ?? measure.Word; // новое слово или бывшее с измерением
                if (measure is MeasureOp)
                    Operator = (measure as MeasureOp).Operator;
            }
            Value = Measure.Value.ToString();

            Autocomplete = new AutocompleteViewModel(
                new Recognizer(Session)
                {
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
        public MeasureEditorViewModel(Measure measure, bool c)
            : this(measure, null, c)
        {
        }

        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="word"></param>
        public MeasureEditorViewModel(Word word, bool c)
            : this(null, word, c)
        {
        }

        /// <summary>
        /// Create
        /// </summary>
        public MeasureEditorViewModel(bool c)
            : this(null, null, c)
        {
        }

        public AutocompleteViewModel Autocomplete { get; private set; }

        public bool WithCompare { get { return withCompare; } }

        public Word Word
        {
            get { return Measure.Word; }
            set { Measure.Word = value; }
        }

        public string Value
        {
            get { return _val; }
            set { _val = value; }
        }

        public MeasureOperator Operator
        {
            get
            {
                return _op;
            }
            set
            {
                if (_op != value)
                {
                    _op = value;

                    OnPropertyChanged(() => Operator);
                }
            }
        }

        public Uom Uom
        {
            get { return Measure.Uom; }
            set { Measure.Uom = value; }
        }

        public Measure Measure { get; private set; }

        public IEnumerable<MeasureOperator> Operators { get { return Enum.GetValues(typeof(MeasureOperator)).Cast<MeasureOperator>(); } }

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
            if (WithCompare)
                (Measure as MeasureOp).Operator = Operator;
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