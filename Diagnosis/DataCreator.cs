using Diagnosis.Models;
using Diagnosis.ViewModels;
using System.Collections.Generic;

namespace Diagnosis
{
    public static class DataCreator
    {
        public static PatientViewModel CreatePatient()
        {
            return new PatientViewModel(new Patient() { })
            {
                FirstName = "Иван",
                LastName = "Грибоедов",
                MiddleName = "Константинович",
                Age = 37,
                IsMale = true
            };
        }

        public static List<Education> CreateEducationList()
        {
            return new List<Education>()
            {
                new Education() { Title = "Высшее"},
                new Education() { Title = "Неоконченное высшее"},
                new Education() { Title = "Среднее"},
            };
        }

        public static List<PropertyViewModel> CreateProperties()
        {
            return new List<PropertyViewModel>()
            {
                (new PropertyViewModel("Образование")).AddValue("Высшее").AddValue("Неоконченное высшее").AddValue("Среднее"),
                (new PropertyViewModel("Характер работы")).AddValue("Сидячая").AddValue("Подвижная"),
                (new PropertyViewModel("Место жительства")).AddValue("Город").AddValue("Деревня"),
            };
        }

        public static List<SymptomViewModel> CreateSymptoms()
        {
            var teeth = new SymptomViewModel("зубная боль");
            teeth.Add(new Symptom("после приёма пищи")).Add(new Symptom("шатается зуб"));

            var throat = new SymptomViewModel(new Symptom()
                {
                    Title = "горло",
                    IsGroup = true
                });
            throat.Add(new SymptomViewModel("боль в горле")
                    .Add(new Symptom("при глотании"))
                    .Add(new Symptom("после разговора")))
                .Add(new Symptom("красное"));

            SymptomViewModel root = (new SymptomViewModel(new Symptom()
            {
                Title = "root",
                IsGroup = true
            })).Add(
                    (new SymptomViewModel(new Symptom()
                    {
                        Title = "голова",
                        IsGroup = true
                    })).Add(throat)
                        .Add(teeth)
                        .Add(new SymptomViewModel(new Symptom("уши")))
                ).Add(
                new SymptomViewModel(new Symptom()
                {
                    Title = "ноги",
                    IsGroup = true
                }));

            root.Initialize();

            return new List<SymptomViewModel>(root.Children);
        }
    }
}