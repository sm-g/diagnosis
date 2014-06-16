using Diagnosis.Data.Repositories;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace Diagnosis.App.ViewModels
{
    static class EntityProducers
    {
        private static DoctorsProducer _doctorsManager;
        private static PropertyProducer _propertyManager;
        private static WordsProducer _wordsManager;
        private static DiagnosisProducer _diagnosisManager;
        private static SymptomsProducer _symptomsManager;

        public static DoctorsProducer DoctorsProducer
        {
            get
            {
                return _doctorsManager ?? (_doctorsManager = new DoctorsProducer(new DoctorRepository()));
            }
        }

        public static PropertyProducer PropertyProducer
        {
            get
            {
                return _propertyManager ?? (_propertyManager = new PropertyProducer(new PropertyRepository()));
            }
        }
        public static WordsProducer WordsProducer
        {
            get
            {
                return _wordsManager ?? (_wordsManager = new WordsProducer(new WordRepository()));
            }
        }
        public static DiagnosisProducer DiagnosisProducer
        {
            get
            {
                return _diagnosisManager ?? (_diagnosisManager = new DiagnosisProducer(new IcdChapterRepository()));
            }
        }
        public static SymptomsProducer SymptomsProducer
        {
            get
            {
                return _symptomsManager ?? (_symptomsManager = new SymptomsProducer(new SymptomRepository()));
            }
        }
    }
}