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

        public string SexSigns
        {
            get { return Get(SexSignsSetting); }
            set { Set(SexSignsSetting, value); }
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