using Diagnosis.Data.Repositories;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace Diagnosis.ViewModels
{
    static class EntityProducers
    {
        private static WordsProducer _wordsManager;
        private static DiagnosisProducer _diagnosisManager;

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
    }
}