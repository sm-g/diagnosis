
namespace Diagnosis.App
{
    /// <summary>
    /// Ключи событий для EventAggregator
    /// </summary>
    static class Messages
    {
        public const string Symptom = "symptomVM";
        public const string CheckedState = "checked";
        public const string Patient = "patientVM";
        public const string Diagnosis = "diagnosisVM";
        public const string Property = "propertyVM";
        public const string Course = "course";
        public const string Appointment = "appointment";
        public const string HealthRecord = "healthRecord";

    }

    enum EventID
    {
        SymptomCheckedChanged,
        CurrentPatientChanged,
        PatientCheckedChanged,
        DiagnosisCheckedChanged,
        PropertySelectedValueChanged,
        CourseStarted,
        AppointmentAdded,
        HealthRecordSelected
    }
}
