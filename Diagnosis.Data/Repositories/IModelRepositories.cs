using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Models;

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
    public interface IPropertyValueRepository : IRepository<PropertyValue>
    {
        PropertyValue GetByValue(string value);
    }
    public interface IPatientPropertyRepository : IRepository<PatientProperty>
    {
        IEnumerable<PatientProperty> GetByPatient(Patient patient);
    }

}
