using Diagnosis.Models;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Patient GetByName(string name);
    }

    public interface IPropertyRepository : IRepository<Property>
    {
        Property GetByTitle(string title);
    }

    public interface IDoctorRepository : IRepository<Doctor>
    {
        Doctor GetByName(string name);
    }

    public interface IWordRepository : IRepository<Word>
    {
        Word GetByTitle(string title);
    }

    public interface ICategoryRepository : IRepository<Category>
    {
        Category GetByTitle(string title);
    }

    public interface ISymptomRepository : IRepository<Symptom>
    {
        IEnumerable<Symptom> GetByWord(Word word);
    }

    public interface IDiagnosisRepository : IRepository<Diagnosis.Models.Diagnosis>
    {
        Diagnosis.Models.Diagnosis GetByTitle(string title);
        Diagnosis.Models.Diagnosis GetByCode(string code);
    }

    public interface IIcdChapterRepository : IRepository<IcdChapter>
    {
        IcdChapter GetByTitle(string title);
        IcdChapter GetByCode(string code);
    }

    public interface IIcdDiseaseRepository : IRepository<IcdDisease>
    {
        IcdDisease GetByTitle(string title);
        IcdDisease GetByCode(string code);
    }
}
