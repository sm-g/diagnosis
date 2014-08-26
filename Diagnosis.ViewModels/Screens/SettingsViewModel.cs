using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;
using EventAggregator;
using Diagnosis.App.Messaging;
using Diagnosis.Core;

namespace Diagnosis.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly Doctor doctor;
        private readonly DoctorViewModel doctorVM;
        private Dictionary<DoctorSettings, Func<bool>> map;

        private bool _onlyTopLevelIcdDisease;
        private bool _showIcdDisease;
        private RelayCommand _save;
        private bool? _dialogResult;

        public SettingsViewModel(DoctorViewModel doctorVM)
        {
            Contract.Requires(doctorVM != null);
            this.doctor = doctorVM.doctor;
            this.doctorVM = doctorVM;

            map = new Dictionary<DoctorSettings, Func<bool>>();
            map.Add(DoctorSettings.ShowIcdDisease, () => ShowIcdDisease);
            map.Add(DoctorSettings.OnlyTopLevelIcdDisease, () => OnlyTopLevelIcdDisease);

            Load();
        }

        public bool? DialogResult
        {
            get
            {
                return _dialogResult;
            }
            set
            {
                if (_dialogResult != value)
                {
                    _dialogResult = value;
                    OnPropertyChanged(() => DialogResult);
                }
            }
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

        public ICommand SaveCommand
        {
            get
            {
                return _save
                    ?? (_save = new RelayCommand(Save));
            }
        }

        public void Reload()
        {
            Load();
            DialogResult = null;
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
                doctorVM.Editable.Commit();
            }

            DialogResult = true;
            this.Send((int)EventID.SettingsSaved, new DoctorModelParams(doctor).Params);
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