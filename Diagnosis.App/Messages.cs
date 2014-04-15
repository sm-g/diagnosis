
namespace Diagnosis.App
{
    /// <summary>
    /// Ключи событий для EventAggregator
    /// </summary>
    static class Messages
    {
        public const string Word = "wordVM";
        public const string CheckedState = "checked";
        public const string Patient = "patientVM";
        public const string Doctor = "doctorVM";
        public const string Diagnosis = "diagnosisVM";
        public const string Property = "propertyVM";
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

        WordsEditingModeChanged
    }
}
