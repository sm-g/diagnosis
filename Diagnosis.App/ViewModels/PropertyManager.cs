using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels
{
    public class PropertyManager : IPropertyManager
    {
        IPropertyRepository propRepo;

        public List<PropertyViewModel> GetPatientProperties(Patient patient)
        {
            Contract.Requires(patient != null);

            var allProperties = propRepo.GetAll();

            var existingPatProps = patient.PatientProperties;
            var properties = new List<PropertyViewModel>(allProperties.Select(p => new PropertyViewModel(p)));

            // указываем значение свойства из БД
            // если у пацента не указано какое-то свойство — добавляем это свойство с пустым значением
            foreach (var propVM in properties)
            {
                var pp = existingPatProps.FirstOrDefault(patProp => patProp.Property == propVM.Property);
                if (pp != null)
                {
                    propVM.SelectedValue = pp.Value;
                }
                else
                {
                    propVM.SelectedValue = new EmptyPropertyValue(propVM.Property);
                }
            }

            return properties;
        }

        public PropertyManager(IPropertyRepository propRepository)
        {
            Contract.Requires(propRepository != null);
            propRepo = propRepository;
        }
    }
}
