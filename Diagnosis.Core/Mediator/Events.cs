using System.Collections.Generic;

namespace Diagnosis.Core
{
    /// <summary>
    /// Ключи событий для EventAggregator
    /// </summary>
    public static class MessageKeys
    {
        public const string Word = "word";
        public const string Words = "words";
        public const string Patient = "patient";
        public const string Doctor = "doctor";
        public const string Diagnosis = "diagnosis";
        public const string Property = "property";
        public const string Course = "course";
        public const string Appointment = "appointment";
        public const string HealthRecord = "healthRecord";
        public const string HealthRecords = "healthRecords";
        public const string Category = "category";
        public const string Boolean = "bool";
        public const string Settings = "settings";

    }

    public class Events
    {
        public const int PropertySelectedValueChanged = 5;

        public const int PatientAdded = 7;
        public const int PatientCreated = 8;

        public const int OpenedPatientChanged = 13;

        public const int OpenHealthRecord = 14;
        public const int OpenSettings = 15;
        public const int OpenPatient = 16;

        public const int SettingsSaved = 17;

        public const int SendToSearch = 18;

    }

    public static class EventAggragatorExtensions
    {
        public static KeyValuePair<string, object>[] AsParams(this object obj, string key)
        {
            return new[] { new KeyValuePair<string, object>(key, obj) };
        }

        public static KeyValuePair<string, object>[] AsParams(this object[] objs, string key)
        {
            return new[] { new KeyValuePair<string, object>(key, objs) };
        }
    }
}
