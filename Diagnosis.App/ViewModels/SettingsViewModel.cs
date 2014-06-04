using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly Doctor doctor;
        private readonly DoctorViewModel doctorVM;

        private bool _onlyTopLevelIcdDisease;

        private RelayCommand _save;
        private bool? _dialogResult;
        Dictionary<DoctorSettings, Func<bool>> map;

        public SettingsViewModel(DoctorViewModel doctorVM)
        {
            Contract.Requires(doctorVM != null);

            this.doctor = doctorVM.doctor;
            this.doctorVM = doctorVM;

            map = new Dictionary<DoctorSettings, Func<bool>>();
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
                doctorVM.Editable.MarkDirty();
            }

            doctorVM.Editable.Commit();

            DialogResult = true;
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