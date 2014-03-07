﻿using Diagnosis.Models;
using Diagnosis.ViewModels;
using System.Collections.Generic;

namespace Diagnosis
{
    public static class DataCreator
    {
        private static List<PatientViewModel> _Patients;

        public static List<PatientViewModel> GetPatients()
        {
            return _Patients ?? (_Patients = new List<PatientViewModel>()
            {
                new PatientViewModel(new Patient())
                {
                    FirstName = "Иван",
                    LastName = "Грибоедов",
                    MiddleName = "Константинович",
                    Age = 37,
                    IsMale = true
                },
                new PatientViewModel(new Patient())
                {
                    FirstName = "Петр",
                    LastName = "Сидоров",
                    MiddleName = "Иванович",
                    Age = 25,
                    IsMale = true
                },
                new PatientViewModel(new Patient())
                {
                    FirstName = "Дарья",
                    LastName = "Смирнова",
                    MiddleName = "Ивановна",
                    Age = 30,
                    IsMale = false
                },
            });
        }

        private static List<PropertyViewModel> _Properties;

        public static List<PropertyViewModel> GetProperties()
        {
            return _Properties ?? (_Properties = new List<PropertyViewModel>()
            {
                (new PropertyViewModel("Образование")).AddValue("Высшее").AddValue("Неоконченное высшее").AddValue("Среднее"),
                (new PropertyViewModel("Характер работы")).AddValue("Сидячая").AddValue("Подвижная"),
                (new PropertyViewModel("Место жительства")).AddValue("Город").AddValue("Деревня"),
            });
        }

        private static List<SymptomViewModel> _Symptoms;

        public static List<SymptomViewModel> Symptoms
        {
            get
            {
                return _Symptoms ?? (_Symptoms = CreateSymptoms());
            }
        }

        private static List<SymptomViewModel> CreateSymptoms()
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