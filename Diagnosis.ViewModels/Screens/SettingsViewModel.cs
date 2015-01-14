using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class SettingsViewModel : DialogViewModel
    {
        private readonly Doctor doctor;
        private Dictionary<DoctorSettings, Func<bool>> map;

        private bool _showIcdDisease;

        public SettingsViewModel(Doctor doctor)
        {
            Contract.Requires(doctor != null);
            this.doctor = doctor;

            map = new Dictionary<DoctorSettings, Func<bool>>();
            map.Add(DoctorSettings.ShowIcdDisease, () => ShowIcdDisease);
            map.Add(DoctorSettings.OnlyTopLevelIcdDisease, () => OnlyTopLevelIcdDisease);
        }

        public bool ShowIcdDisease
        {
            get
            {
                return _showIcdDisease;
            }
            set
            {
                if (_showIcdDisease != value)
                {
                    _showIcdDisease = value;
                    OnPropertyChanged(() => ShowIcdDisease);
                }
            }
        }

        public bool OnlyTopLevelIcdDisease
        {
            get
            {
                return doctor.Settings.IcdTopLevelOnly;
            }
            set
            {
                doctor.Settings.IcdTopLevelOnly = value;
                OnPropertyChanged(() => OnlyTopLevelIcdDisease);
            }
        }

        protected override void OnOk()
        {
            new Saver(Session).Save(doctor);
            this.Send(Event.SettingsSaved, doctor.AsParams(MessageKeys.Doctor));
        }
    }
}