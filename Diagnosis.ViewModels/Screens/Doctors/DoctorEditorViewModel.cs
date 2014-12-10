using Diagnosis.Common;
using Diagnosis.Models;
using log4net;
using System.ComponentModel;
using NHibernate.Linq;
using System.Linq;
using System.Collections.Generic;

namespace Diagnosis.ViewModels.Screens
{
    public class DoctorEditorViewModel : DialogViewModel
    {
        private List<Speciality> _specialities;
        private static readonly ILog logger = LogManager.GetLogger(typeof(DoctorEditorViewModel));
        private readonly Doctor doctor;
        private DoctorViewModel _vm;

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

        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    (doctor as IEditableObject).EndEdit();


                    using (var t = Session.BeginTransaction())
                    {
                        try
                        {
                            Session.SaveOrUpdate(doctor);
                            t.Commit();
                        }
                        catch (System.Exception e)
                        {
                            t.Rollback();
                            logger.Error(e);
                        }
                    }

                    this.Send(Events.DoctorSaved, doctor.AsParams(MessageKeys.Doctor));
                    DialogResult = true;
                }, () => CanSave());
            }
        }

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

            Title = "Данные врача";
        }

        /// <summary>
        /// Начинает редактировать нового врача.
        /// </summary>
        public DoctorEditorViewModel()
            : this(new Doctor("Врач"))
        { }

        private bool CanSave()
        {
            return (doctor.IsTransient || doctor.IsDirty) && doctor.IsValid(); // есть изменения или при создании
        }

        protected override void OnCancel()
        {
            (doctor as IEditableObject).CancelEdit();
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