using Diagnosis.Data;
using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CriteriaGroupEditorViewModel : DialogViewModel, ICritKeeper
    {
        internal readonly CriteriaGroup crgroup;
        private ExistanceTester<Models.CriteriaGroup> tester;

        public CriteriaGroupEditorViewModel(CriteriaGroup group)
        {
            this.crgroup = group;

            Criteria = new ObservableCollection<CriterionEditorViewModel>();

            CriteriaGroup = new CriteriaGroupViewModel(crgroup);
            tester = new ExistanceTester<CriteriaGroup>(crgroup, CriteriaGroup, Session);
            tester.Test();

            (crgroup as IEditableObject).BeginEdit();

            Title = "Редактор группы критериев";
            HelpTopic = "";
            WithHelpButton = false;
        }

        public CriteriaGroupViewModel CriteriaGroup { get; private set; }

        public ObservableCollection<CriterionEditorViewModel> Criteria { get; private set; }

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
            get { return crgroup.IsValid() && !CriteriaGroup.HasExistingValue; }
        }

        protected override void OnOk()
        {
            (crgroup as IEditableObject).EndEdit();

            Session.DoDelete(crgroup);
        }

        protected override void OnCancel()
        {
            (crgroup as IEditableObject).CancelEdit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                tester.Dispose();
            }
            base.Dispose(disposing);
        }

        public ICrit Crit
        {
            get { return crgroup; }
        }
    }
}