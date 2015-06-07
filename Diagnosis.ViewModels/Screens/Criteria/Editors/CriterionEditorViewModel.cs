using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using System;
using System.ComponentModel;

using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CriterionEditorViewModel : DialogViewModel, ICritKeeper
    {
        internal readonly Criterion criterion;
        private OptionsLoader loader;
        private ExistanceTester<Models.Criterion> tester;

        public CriterionEditorViewModel(Criterion cr)
        {
            this.criterion = cr;
            loader = new JsonOptionsLoader(Session);

            QueryEditor = new QueryEditorViewModel(Session);

            Criterion = new CriterionViewModel(criterion);
            tester = new ExistanceTester<Criterion>(cr, Criterion, Session);
            tester.Test();

            var opt = loader.ReadOptions(criterion.Options);
            QueryEditor.SetOptions(opt);

            (criterion as IEditableObject).BeginEdit();

            Title = "Редактор критерия";
            HelpTopic = "editcriterion";
            WithHelpButton = false;
        }

        public CriterionViewModel Criterion { get; private set; }

        public QueryEditorViewModel QueryEditor { get; private set; }

        public override bool CanOk
        {
            get { return criterion.IsValid() && !Criterion.HasExistingValue; }
        }

        protected override void OnOk()
        {
            var opt = QueryEditor.GetOptions();
            criterion.Options = loader.WriteOptions(opt);
            criterion.OptionsFormat = loader.Format;

            var words = opt.GetAllWords().ToArray();
            criterion.SetWords(words);

            (criterion as IEditableObject).EndEdit();

            if (AuthorityController.CurrentDoctor != null)
                AuthorityController.CurrentDoctor.AddWords(words);

            Session.DoSave(words);
            Session.DoSave(criterion);
        }

        protected override void OnCancel()
        {
            (criterion as IEditableObject).CancelEdit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                tester.Dispose();
                Criterion.Dispose();
            }
            base.Dispose(disposing);
        }

        public ICrit Crit
        {
            get { return criterion; }
        }
    }
}