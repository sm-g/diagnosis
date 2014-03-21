using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Diagnosis.Models;

namespace Diagnosis.ViewModels
{
    public interface IPropertyManager
    {
        List<PropertyViewModel> GetPatientProperties(Patient patient);
    }
}
