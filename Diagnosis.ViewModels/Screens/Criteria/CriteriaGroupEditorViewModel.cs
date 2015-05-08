using Diagnosis.Data;
using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CriteriaGroupEditorViewModel : DialogViewModel
    {
        internal readonly CriteriaGroup crgroup;

        public CriteriaGroupEditorViewModel(CriteriaGroup group)
        {
            this.crgroup = group;

            crgroup.CriteriaChanged += crgroup_CriteriaChanged;
            Criteria = new ObservableCollection<CriterionEditorViewModel>();

            CriteriaGroup = new CriteriaGroupViewModel(crgroup);

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
            get { return true; }
        }

        protected override void OnOk()
        {
            (crgroup as IEditableObject).EndEdit();

            new Saver(Session).Save(crgroup);
        }

        protected override void OnCancel()
        {
            (crgroup as IEditableObject).CancelEdit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                crgroup.CriteriaChanged -= crgroup_CriteriaChanged;
            }
            base.Dispose(disposing);
        }

        private void crgroup_CriteriaChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}