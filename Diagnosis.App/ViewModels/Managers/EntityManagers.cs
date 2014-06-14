using Diagnosis.Data.Repositories;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace Diagnosis.App.ViewModels
{
    static class EntityManagers
    {
        private static DoctorsManager _doctorsManager;
        private static PatientsManager _patientsManager;
        private static PropertyManager _propertyManager;
        private static WordsManager _wordsManager;
        private static DiagnosisManager _diagnosisManager;
        private static SymptomsManager _symptomsManager;

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
        public static WordsManager WordsManager
        {
            get
            {
                return _wordsManager ?? (_wordsManager = new WordsManager(new WordRepository()));
            }
        }
        public static DiagnosisManager DiagnosisManager
        {
            get
            {
                return _diagnosisManager ?? (_diagnosisManager = new DiagnosisManager(new IcdChapterRepository()));
            }
        }
        public static SymptomsManager SymptomsManager
        {
            get
            {
                return _symptomsManager ?? (_symptomsManager = new SymptomsManager(new SymptomRepository()));
            }
        }
    }
}