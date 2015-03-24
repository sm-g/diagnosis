using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using log4net;
using NHibernate.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class DoctorEditorViewModel : DialogViewModel
    {
        protected Doctor doctor;
        private static readonly ILog logger = LogManager.GetLogger(typeof(DoctorEditorViewModel));
        private List<Speciality> _specialities;
        private DoctorViewModel _vm;

        /// <summary>
        /// Начинает редактировать врача.
        /// </summary>
        public DoctorEditorViewModel(Doctor doctor)
        {
            this.doctor = doctor;

            _specialities = new List<Speciality> { Speciality.Null };
            _specialities.AddRange(Session.Query<Speciality>()
                .OrderBy(s => s.Title));

            Doctor = new DoctorViewModel(doctor);
            (doctor as IEditableObject).BeginEdit();
            Doctor.canEditComboBoxValues = true;

            Title = "Данные врача";
            HelpTopic = "editdoctor";
            WithHelpButton = false;

        }

        /// <summary>
        /// Начинает редактировать нового врача.
        /// </summary>
        public DoctorEditorViewModel()
            : this(new Doctor("Врач"))
        { }

        public DoctorViewModel Doctor
        {
            get
            {
                return _vm;
            }
            private set
            {
                if (_vm != value)
                {
                    _vm = value;
                    OnPropertyChanged(() => Doctor);
                }
            }
        }

        public IEnumerable<Speciality> Specialities { get { return _specialities; } }

        public override bool CanOk
        {
            get
            {
                return (doctor.IsTransient || doctor.IsDirty) && doctor.IsValid(); // есть изменения или при создании
            }
        }

        protected override void OnOk()
        {
            (doctor as IEditableObject).EndEdit();
            Doctor.canEditComboBoxValues = false;

            new Saver(Session).Save(doctor);

            this.Send(Event.DoctorSaved, doctor.AsParams(MessageKeys.Doctor));
        }

        protected override void OnCancel()
        {
            (doctor as IEditableObject).CancelEdit();
            Doctor.canEditComboBoxValues = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Doctor.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}