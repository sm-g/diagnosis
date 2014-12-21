using Diagnosis.Data;
using Diagnosis.Models;
using log4net;
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;

namespace Diagnosis.ViewModels.Screens
{
    public class AppointmentEditorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AppointmentEditorViewModel));
        private readonly Appointment app;

        public AppointmentEditorViewModel(Appointment app)
        {
            this.app = app;
            this.validatableEntity = app;
            columnToPropertyMap = new Dictionary<string, string>() {
                {"Date", "DateAndTime"},
                {"Time", "DateAndTime"}
            };

            (app as IEditableObject).BeginEdit();

            Title = "Редактирование осмотра";
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}