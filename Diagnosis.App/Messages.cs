﻿
namespace Diagnosis.App.Messaging
{
    /// <summary>
    /// Ключи событий для EventAggregator
    /// </summary>
    static class Messages
    {
        public const string Word = "word";
        public const string CheckedState = "checked";
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

    enum EventID
    {
        CategoryCheckedChanged,
        WordCheckedChanged,
        PatientCheckedChanged,
        DiagnosisCheckedChanged,

        CurrentPatientChanged,
        CurrentDoctorChanged,
        PropertySelectedValueChanged,
        HealthRecordSelected,

        CourseStarted,
        AppointmentAdded,

        WordsEditingModeChanged,
        HealthRecordChanged,

        OpenHealthRecord,
        OpenSettings
    }
}
