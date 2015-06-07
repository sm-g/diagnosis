using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class EstimatorEditorViewModel : DialogViewModel, ICritKeeper
    {
        internal readonly Estimator estimator;

        private OptionsLoader loader;

        public EstimatorEditorViewModel(Estimator estimator)
        {
            this.estimator = estimator;
            loader = new JsonOptionsLoader(Session);

            CriteriaGroups = new ObservableCollection<CriteriaGroupEditorViewModel>();

            QueryEditor = new QueryEditorViewModel(Session);

            var opt = loader.ReadOptions(estimator.Options);
            QueryEditor.SetOptions(opt);
            Estimator = new EstimatorViewModel(estimator);

            (estimator as IEditableObject).BeginEdit();

            Title = "Редактор приказа";
            HelpTopic = "";
            WithHelpButton = false;
        }

        public EstimatorViewModel Estimator { get; private set; }

        public QueryEditorViewModel QueryEditor { get; private set; }

        public ObservableCollection<CriteriaGroupEditorViewModel> CriteriaGroups { get; private set; }

        public RelayCommand AddCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                });
            }
        }

        public RelayCommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                });
            }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                });
            }
        }

        public override bool CanOk
        {
            get { return estimator.IsValid() && !Estimator.HasExistingValue; }
        }

        protected override void OnOk()
        {
            var opt = QueryEditor.GetOptions();
            estimator.Options = loader.WriteOptions(opt);
            estimator.OptionsFormat = loader.Format;

            var words = opt.GetAllWords().ToArray();
            estimator.SetWords(words);

            (estimator as IEditableObject).EndEdit();

            if (AuthorityController.CurrentDoctor != null)
                AuthorityController.CurrentDoctor.AddWords(words);

            Session.DoSave(words);
            Session.DoSave(estimator);
        }

        protected override void OnCancel()
        {
            (estimator as IEditableObject).CancelEdit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }

        public ICrit Crit
        {
            get { return estimator; }
        }
    }
}