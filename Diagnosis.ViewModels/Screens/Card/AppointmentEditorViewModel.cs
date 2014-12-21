using Diagnosis.Data;
using Diagnosis.Models;
using log4net;
using System;
using System.Linq;
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

        public DateTime Time
        {
            get { return app.DateAndTime; }
            set { app.DateAndTime = app.DateAndTime.Date.Add(value.TimeOfDay); }
        }

        public DateTime Date
        {
            get { return app.DateAndTime.Date; }
            set { app.DateAndTime = value.Add(app.DateAndTime.TimeOfDay); }
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

            new Saver(Session).Save(app);
        }

        protected override void OnCancel()
        {
            (app as IEditableObject).CancelEdit();
        }
        public override string this[string columnName]
        {
            get
            {
                var results = app.SelfValidate();
                if (results == null)
                    return string.Empty;
                var message = results.Errors
                    .Where(x => x.PropertyName == columnName)
                    .Select(x => x.ErrorMessage)
                    .FirstOrDefault();
                return message != null ? message : string.Empty;
            }
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