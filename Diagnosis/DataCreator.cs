using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.ViewModels;
using Diagnosis.Models;


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
            SymptomViewModel root1 = new SymptomViewModel(new Symptom()
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
                    new SymptomViewModel(new Symptom()
                    {
                        Title = "больные зубы"
                    })
                    {
                        Children = {
                            new SymptomViewModel(new Symptom()
                            {
                                Title = "боль в зубе"
                            }),
                            new SymptomViewModel(new Symptom()
                            {
                                Title = "шатается зуб"
                            })
                        }
                    },
                    new SymptomViewModel(new Symptom()
                    {
                        Title = "уши"
                    }),
                }
            };
            SymptomViewModel root2 = new SymptomViewModel(new Symptom()
            {
                Title = "ноги",
                IsGroup = true

            });

            root1.Initialize();
            root2.Initialize();

            return new List<SymptomViewModel>() { root1, root2 };
        }
    }
}
