using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;
using EventAggregator;
using Diagnosis.Common;
using Diagnosis.Data.Repositories;

namespace Diagnosis.ViewModels.Screens
{
    public class SettingsViewModel : DialogViewModel
    {
        private readonly Doctor doctor;
        private Dictionary<DoctorSettings, Func<bool>> map;

        private bool _onlyTopLevelIcdDisease;
        private bool _showIcdDisease;

        public SettingsViewModel(Doctor doctor)
        {
            Contract.Requires(doctor != null);
            this.doctor = doctor;

            map = new Dictionary<DoctorSettings, Func<bool>>();
            map.Add(DoctorSettings.ShowIcdDisease, () => ShowIcdDisease);
            map.Add(DoctorSettings.OnlyTopLevelIcdDisease, () => OnlyTopLevelIcdDisease);

            Load();
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
                return _onlyTopLevelIcdDisease;
            }
            set
            {
                if (_onlyTopLevelIcdDisease != value)
                {
                    _onlyTopLevelIcdDisease = value;
                    OnPropertyChanged(() => OnlyTopLevelIcdDisease);
                }
            }
        }

        protected override void OnOk()
        {
            Save();
        }

        private void SetFlag(DoctorSettings flag, bool value)
        {
            if (value)
            {
                doctor.DoctorSettings |= flag;
            }
            else
            {
                doctor.DoctorSettings &= ~flag;
            }
        }

        private void Load()
        {
            ShowIcdDisease = doctor.DoctorSettings.HasFlag(DoctorSettings.ShowIcdDisease);
            OnlyTopLevelIcdDisease = doctor.DoctorSettings.HasFlag(DoctorSettings.OnlyTopLevelIcdDisease);
        }

        private void Save()
        {
            var changed = ChangedFlags();
            foreach (var flag in changed)
            {
                SetFlag(flag, map[flag]());
            }

            if (changed.Count > 0)
            {
                Session.SaveOrUpdate(doctor);
            }

            this.Send(Events.SettingsSaved, doctor.AsParams(MessageKeys.Doctor));
        }

        private IList<DoctorSettings> ChangedFlags()
        {
            List<DoctorSettings> result = new List<DoctorSettings>();

            result.AddRange(map
                .Where(kvp => doctor.DoctorSettings.HasFlag(kvp.Key) != kvp.Value())
                .Select(kvp => kvp.Key));

            return result;
        }
    }
}