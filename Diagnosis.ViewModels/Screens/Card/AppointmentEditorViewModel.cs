using Diagnosis.Models;
using log4net;
using System.ComponentModel;

namespace Diagnosis.ViewModels.Screens
{
    public class AppointmentEditorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AppointmentEditorViewModel));
        private readonly Appointment app;

        private AppointmentViewModel _appVm;

        public AppointmentEditorViewModel(Appointment app)
        {
            this.app = app;
            Appointment = new AppointmentViewModel(app);
            (app as IEditableObject).BeginEdit();

            Title = "Редактирование осмотра";
        }

        public AppointmentViewModel Appointment
        {
            get
            {
                return _appVm;
            }
            set
            {
                if (_appVm != value)
                {
                    _appVm = value;
                    OnPropertyChanged(() => Appointment);
                }
            }
        }

        public override bool CanOk
        {
            get
            {
                return app.IsValid();
            }
        }

        protected override void OnOk()
        {
            (app as IEditableObject).EndEdit();

            using (var t = Session.BeginTransaction())
            {
                try
                {
                    t.Commit();
                }
                catch (System.Exception e)
                {
                    t.Rollback();
                    logger.Error(e);
                }
            }
        }

        protected override void OnCancel()
        {
            (app as IEditableObject).CancelEdit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Appointment.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}