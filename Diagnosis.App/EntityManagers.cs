using Diagnosis.App.ViewModels;
using Diagnosis.Data.Repositories;

namespace Diagnosis.App
{
    internal static class EntityManagers
    {
        private static DoctorsManager _doctorsManager;
        private static PatientsManager _patientsManager;
        private static PropertyManager _propertyManager;
        private static SymptomsManager _symptomsManager;
        private static DiagnosisManager _diagnosisManager;

        public static DoctorsManager DoctorsManager
        {
            get
            {
                return _doctorsManager ?? (_doctorsManager = new DoctorsManager(new DoctorRepository()));
            }
        }

        public static PatientsManager PatientsManager
        {
            get
            {
                return _patientsManager ?? (_patientsManager = new PatientsManager(new PatientRepository()));
            }
        }

        public static PropertyManager PropertyManager
        {
            get
            {
                return _propertyManager ?? (_propertyManager = new PropertyManager(new PropertyRepository()));
            }
        }
        public static SymptomsManager SymptomsManager
        {
            get
            {
                return _symptomsManager ?? (_symptomsManager = new SymptomsManager(new SymptomRepository()));
            }
        }
        public static DiagnosisManager DiagnosisManager
        {
            get
            {
                return _diagnosisManager ?? (_diagnosisManager = new DiagnosisManager(new DiagnosisRepository()));
            }
        }
    }
}