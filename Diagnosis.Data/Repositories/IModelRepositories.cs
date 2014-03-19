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

    public interface ISymptomRepository : IRepository<Symptom>
    {
        Symptom GetByTitle(string title);
    }

    public interface IDiagnosisRepository : IRepository<Diagnosis.Models.Diagnosis>
    {
        Diagnosis.Models.Diagnosis GetByTitle(string title);
        Diagnosis.Models.Diagnosis GetByCode(string code);
    }
}
