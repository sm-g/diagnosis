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
        private ExistanceTester<Models.Vocabulary> tester;

        public VocabularyEditorViewModel(Vocabulary voc)
        {
            Contract.Requires(voc != null);

            this.voc = voc;
            (voc as IEditableObject).BeginEdit();

            Templates = new ObservableCollection<string>();
            TooLongTemplates = new ObservableCollection<string>();

            Vocabulary = new VocabularyViewModel(voc);
            tester = new ExistanceTester<Vocabulary>(voc, Vocabulary, Session);
            tester.Test();

            Title = "Редактор словаря";
            HelpTopic = "editVocabulary";
            WithHelpButton = false;

            TemplatesString = string.Join(Environment.NewLine, voc.GetOrderedTemplateTitles());
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

        public int MaxLength { get { return Length.WordTitle; } }

        public override bool CanOk
        {
            get
            {
                return voc.IsValid() && !Vocabulary.HasExistingValue;
            }
        }

        private void MakeTemplates()
        {
            var temps = _templates
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var tooLong = temps.Where(x => x.Length > MaxLength);

            Templates.SyncWith(temps.Select(x => x.Truncate(MaxLength)));
            TooLongTemplates.SyncWith(tooLong);

            TemplatesCount = Templates.Count;
        }

        protected override void OnOk()
        {
            voc.SetTemplates(Templates);
            (voc as IEditableObject).EndEdit();

            Session.DoDelete(voc);
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
                tester.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}