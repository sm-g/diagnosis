using Diagnosis.Core;
using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly Doctor doctor;
        private readonly DoctorViewModel doctorVM;

        private bool _onlyTopLevelIcdDisease;

        private RelayCommand _save;
        private bool? _dialogResult;

        public SettingsViewModel(DoctorViewModel doctorVM)
        {
            Contract.Requires(doctorVM != null);

            this.doctor = doctorVM.doctor;
            this.doctorVM = doctorVM;
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
            SetFlag(DoctorSettings.OnlyTopLevelIcdDisease, OnlyTopLevelIcdDisease);
            doctorVM.Editable.Commit(true);
            DialogResult = true;
        }
    }
}