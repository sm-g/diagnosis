using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.App.ViewModels;
using Diagnosis.Data.Repositories;

namespace Diagnosis.App
{
    static class EntityManagers
    {
        static PropertyManager _propertyManager;
        public static PropertyManager PropertyManager
        {
            get
            {
                return _propertyManager ?? (_propertyManager = new PropertyManager(new PropertyRepository()));
            }
        }

        static SymptomsManager _symptomsManager;
        public static SymptomsManager SymptomsManager
        {
            get
            {
                return _symptomsManager ?? (_symptomsManager = new SymptomsManager(new SymptomRepository()));
            }
        }
    }
}