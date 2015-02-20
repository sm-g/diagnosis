using System;
using System.Linq;

namespace Diagnosis.Models
{
    public class SettingsProvider
    {
        public readonly Doctor doctor;

        private static string IcdTopLevelOnlySetting = "IcdTopLevelOnly";
        private static string HrListSortingSetting = "HrListSorting";
        private static string HrListGroupingSetting = "HrListGrouping";
        private static string SexSignsSetting = "SexSigns";
        private static string FontSizesSetting = "FontSizes";
        private static string PatientsListSortingSetting = "PatientsListSorting";
        private static string PatientsListSortingDirectionSetting = "PatientsListSortingDirection";

        public SettingsProvider(Doctor doc)
        {
            doctor = doc;
        }

        public bool IcdTopLevelOnly
        {
            get
            {
                bool res = false;
                bool.TryParse(Get(IcdTopLevelOnlySetting), out res);
                return res;
            }
            set { Set(IcdTopLevelOnlySetting, value.ToString()); }
        }

        public string HrListGrouping
        {
            get { return Get(HrListGroupingSetting); }
            set { Set(HrListGroupingSetting, value.ToString()); }
        }

        public string HrListSorting
        {
            get { return Get(HrListSortingSetting); }
            set { Set(HrListSortingSetting, value.ToString()); }
        }

        public string PatientsListSorting
        {
            get { return Get(PatientsListSortingSetting); }
            set { Set(PatientsListSortingSetting, value.ToString()); }
        }

        public string PatientsListSortingDirection
        {
            get { return Get(PatientsListSortingDirectionSetting); }
            set { Set(PatientsListSortingDirectionSetting, value.ToString()); }
        }

        public string SexSigns
        {
            get { return Get(SexSignsSetting); }
            set { Set(SexSignsSetting, value); }
        }

        public bool BigFontSize
        {
            get
            {
                bool res = false;
                bool.TryParse(Get(FontSizesSetting), out res);
                return res;
            }
            set { Set(FontSizesSetting, value.ToString()); }
        }

        private string Get(string setting)
        {
            var set = doctor.SettingsSet.FirstOrDefault(s => s.Title.Equals(setting, StringComparison.InvariantCultureIgnoreCase));
            return set != null ? set.Value : null;
        }

        private void Set(string setting, string value)
        {
            var set = doctor.SettingsSet.FirstOrDefault(s => s.Title.Equals(setting, StringComparison.InvariantCultureIgnoreCase));
            if (set == null)
            {
                doctor.SettingsSet.Add(new Setting(doctor, setting) { Value = value });
            }
            else
            {
                set.Value = value;
            }
        }
    }
}