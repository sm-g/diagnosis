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

        public static List<SymptomViewModel> CreateSymptoms()
        {
            var teeth = new SymptomViewModel("больные зубы");
            teeth.Add(new Symptom()
                                    {
                                        Title = "боль в зубе"
                                    });

            teeth.Add(new Symptom()
                                    {
                                        Title = "шатается зуб"
                                    });

            SymptomViewModel root = new SymptomViewModel(new Symptom()
            {
                Title = "root",
                IsGroup = true
            })
            {
                Children =
                {
                    new SymptomViewModel(new Symptom()
                    {
                        Title = "голова",
                        IsGroup = true
                    })
                    {
                        Children =
                        {
                            new SymptomViewModel(new Symptom()
                            {
                                Title = "нос"
                            }),
                            teeth,
                            new SymptomViewModel(new Symptom()
                            {
                                Title = "уши"
                            }),
                        }
                    },
                    new SymptomViewModel(new Symptom()
                    {
                        Title = "ноги",
                        IsGroup = true
                    })
                }
            };

            root.Initialize();

            return new List<SymptomViewModel>(root.Children);
        }
    }
}