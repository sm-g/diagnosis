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

            QueryEditor = new QueryEditorViewModel(Session, () => { });

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
            criterion.Options = loader.WriteOptions(QueryEditor.GetOptions());
            (criterion as IEditableObject).EndEdit();

            new Saver(Session).Save(criterion);
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