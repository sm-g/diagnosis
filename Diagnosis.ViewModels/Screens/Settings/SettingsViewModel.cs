using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class SettingsViewModel : DialogViewModel
    {
        private readonly Doctor doctor;
        private bool _showIcdDisease;
        private bool _onlyTopLevelIcdDisease;
        private string _selSex;
        private bool _bigFont;

        public SettingsViewModel(Doctor doctor)
        {
            Contract.Requires(doctor != null);
            this.doctor = doctor;

            Sexes = new ObservableCollection<string>() {
                "М Ж ?",
                "Муж Жен ?",
                "1 2 ?"
            };

            BigFont = doctor.Settings.BigFontSize;
            OnlyTopLevelIcdDisease = doctor.Settings.IcdTopLevelOnly;
            SelectedSex = doctor.Settings.SexSigns ?? Sexes[0];

            Title = "Настройки";
            HelpTopic = "doctorsettings";
            WithHelpButton = false;
        }

        public ObservableCollection<string> Sexes { get; private set; }

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
                _onlyTopLevelIcdDisease = value;
                OnPropertyChanged(() => OnlyTopLevelIcdDisease);
            }
        }

        public bool BigFont
        {
            get
            {
                return _bigFont;
            }
            set
            {
                _bigFont = value;

                // live preview
                this.Send(Event.ChangeFont, value.AsParams(MessageKeys.Boolean));
                OnPropertyChanged(() => BigFont);
            }
        }
        public string SelectedSex
        {
            get
            {
                return _selSex;
            }
            set
            {
                _selSex = value;
                OnPropertyChanged(() => SelectedSex);
            }
        }

        protected override void OnOk()
        {
            doctor.Settings.BigFontSize = BigFont;
            doctor.Settings.IcdTopLevelOnly = OnlyTopLevelIcdDisease;
            doctor.Settings.SexSigns = SelectedSex;

            Session.DoSave(doctor);
            this.Send(Event.EntitySaved, doctor.AsParams(MessageKeys.Entity));
        }

        protected override void OnCancel()
        {
            // restore font size
            this.Send(Event.ChangeFont, doctor.Settings.BigFontSize.AsParams(MessageKeys.Boolean));
        }
    }
}