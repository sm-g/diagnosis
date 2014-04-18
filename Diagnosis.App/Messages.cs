
namespace Diagnosis.App
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
        public const string Boolean = "bool";

    }

    enum EventID
    {
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

        HealthRecordChanged
    }
}
