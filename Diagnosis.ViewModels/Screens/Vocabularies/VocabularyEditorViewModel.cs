using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using log4net;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class VocabularyEditorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(VocabularyEditorViewModel));

        private Vocabulary voc;
        private string _templates;
        private int count;

        public VocabularyEditorViewModel(Vocabulary voc)
        {
            Contract.Requires(voc != null);

            this.voc = voc;
            (voc as IEditableObject).BeginEdit();

            Templates = new ObservableCollection<string>();
            TooLongTemplates = new ObservableCollection<string>();

            var vocs = Session.Query<Vocabulary>()
                .ToList();
            Vocabulary = new VocabularyViewModel(voc);
            Vocabulary.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Title")
                {
                    TestExisting(Vocabulary, vocs);
                }
            };
            TestExisting(Vocabulary, vocs);

            Title = "Редактор словаря";
            HelpTopic = "editVocabulary";
            WithHelpButton = false;

            TemplatesString = string.Join(Environment.NewLine, voc.WordTemplates
                .Select(x => x.Title)
                .OrderBy(x => x));
        }

        public VocabularyViewModel Vocabulary { get; set; }

        /// <summary>
        /// Получившиеся шаблоны.
        /// </summary>
        public ObservableCollection<string> Templates { get; private set; }

        public ObservableCollection<string> TooLongTemplates { get; private set; }

        /// <summary>
        /// Шаблоны редактируются как одна строка.
        /// </summary>
        public string TemplatesString
        {
            get
            {
                return _templates;
            }
            set
            {
                if (_templates != value)
                {
                    _templates = value;

                    MakeTemplates();

                    OnPropertyChanged(() => TemplatesString);
                }
            }
        }
        public int TemplatesCount
        {
            get
            {
                return count;
            }
            set
            {
                if (count != value)
                {
                    count = value;
                    OnPropertyChanged(() => TemplatesCount);
                }
            }
        }

        public int MaxLength { get { return WordTemplate.MaxLength; } }

        public override bool CanOk
        {
            get
            {
                return voc.IsValid() && !Vocabulary.HasExistingTitle;
            }
        }

        private void MakeTemplates()
        {
            var temps = _templates
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();
            var tooLong = temps.Where(x => x.Length > WordTemplate.MaxLength);

            Templates.SyncWith(temps.Select(x => x.Truncate(WordTemplate.MaxLength)));
            TooLongTemplates.SyncWith(tooLong);

            TemplatesCount = Templates.Count;
        }

        protected override void OnOk()
        {
            voc.SetTemplates(Templates);
            (voc as IEditableObject).EndEdit();

            new Saver(Session).Save(voc);
        }

        protected override void OnCancel()
        {
            (voc as IEditableObject).CancelEdit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Vocabulary.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Нельзя ввести словарь, который уже есть.
        /// </summary>
        private void TestExisting(VocabularyViewModel vm, IEnumerable<Vocabulary> vocs)
        {
            vm.HasExistingTitle = vocs.Any(s => s.Title == voc.Title && s != voc);
        }
    }
}