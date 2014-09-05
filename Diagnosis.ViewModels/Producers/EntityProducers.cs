using Diagnosis.Data.Repositories;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace Diagnosis.ViewModels
{
    static class EntityProducers
    {
        private static DiagnosisProducer _diagnosisManager;

        public static DiagnosisProducer DiagnosisProducer
        {
            get
            {
                return _diagnosisManager ?? (_diagnosisManager = new DiagnosisProducer(new IcdChapterRepository()));
            }
        }
    }
}