using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class HealthRecordViewModel : ViewModelBase
    {
        internal readonly HealthRecord healthRecord;
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HealthRecordViewModel));

        public HealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            this.healthRecord = hr;

            DateEditor = new DateEditorViewModel(hr);
        }

        public HrCategory Category
        {
            get
            {
                return healthRecord.Category;
            }
            set
            {
                healthRecord.Category = value;
            }
        }

        public DateEditorViewModel DateEditor { get; private set; }

        public ICommand SendToSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.SendToSearch, healthRecord.ToEnumerable().AsParams(MessageKeys.ToSearchPackage));
                });
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", GetType().Name, healthRecord);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DateEditor.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}