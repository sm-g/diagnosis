﻿
namespace Diagnosis.Core
{
    /// <summary>
    /// Ключи событий для EventAggregator
    /// </summary>
    public static class Messages
    {
        public const string Word = "word";
        public const string Patient = "patient";
        public const string Doctor = "doctor";
        public const string Diagnosis = "diagnosis";
        public const string Property = "property";
        public const string Course = "course";
        public const string Appointment = "appointment";
        public const string HealthRecord = "healthRecord";
        public const string Category = "category";
        public const string Boolean = "bool";
        public const string Settings = "settings";

    }

    public enum EventID
    {
        CategoryCheckedChanged,
        WordCheckedChanged,
        DiagnosisCheckedChanged,

        CurrentDoctorChanged,
        PropertySelectedValueChanged,
        HealthRecordSelected,

        PatientAdded,
        PatientCreated,
        CourseStarted,
        AppointmentAdded,

        WordsEditingModeChanged,
        HealthRecordChanged,

        OpenedPatientChanged,

        OpenHealthRecord,
        OpenSettings,
        OpenPatient,

        SettingsSaved,

        SendToSearch
    }
}