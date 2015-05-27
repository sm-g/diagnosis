using Diagnosis.Data;
using Diagnosis.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class AppointmentEditorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AppointmentEditorViewModel));
        private readonly Appointment app;

        public AppointmentEditorViewModel(Appointment app)
        {
            this.app = app;

            app.PropertyChanged += app_PropertyChanged;
            (app as IEditableObject).BeginEdit();
            (app.Course as IEditableObject).BeginEdit();

            Appointment = new AppointmentViewModel(app);

            Title = "Редактирование осмотра";
            HelpTopic = "editholder";
            WithHelpButton = true;

        }
        public AppointmentViewModel Appointment { get; private set; }
        public RelayCommand CorrectCourseDatesCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    app.Course.FitDatesToApps();
                    OnPropertyChanged(() => Appointment);
                    OnPropertyChanged(() => DateTimeInvalid);
                });
            }
        }

        public bool DateTimeInvalid
        {
            get
            {
                return !app.IsValid() && app.GetErrors().Any(x => x.PropertyName.Contains("DateAndTime"));
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
            (app.Course as IEditableObject).EndEdit();
            (app as IEditableObject).EndEdit();

            Session.Persist0(app, app.Course);
        }

        protected override void OnCancel()
        {
            (app.Course as IEditableObject).CancelEdit();
            (app as IEditableObject).CancelEdit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                app.PropertyChanged -= app_PropertyChanged;
                Appointment.Dispose();
            }
            base.Dispose(disposing);
        }

        private void app_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(() => DateTimeInvalid);
        }
    }
}