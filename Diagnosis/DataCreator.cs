using Diagnosis.Models;
using Diagnosis.ViewModels;
using System.Collections.Generic;

namespace Diagnosis
{
    public static class DataCreator
    {
        private static List<DoctorViewModel> _Doctors;

        public static List<DoctorViewModel> GetDoctors()
        {
            return _Doctors ?? (_Doctors = new List<DoctorViewModel>()
            {
                new DoctorViewModel(new Doctor())
                {
                    FirstName = "Иван",
                    LastName = "Охлобыстин",
                    MiddleName = "Иванович",
                    Speciality = "Хирург"

                },
                new DoctorViewModel(new Doctor())
                {
                    FirstName = "Петр",
                    LastName = "Сидоров",
                    MiddleName = "Иванович",
                    Speciality = "Невролог"

                }
            });
        }

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
                    Age = 37
                },
                new PatientViewModel(new Patient())
                {
                    FirstName = "Петр",
                    LastName = "Сидоров",
                    MiddleName = "Иванович",
                    Age = 25
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
            teeth.Add(new SymptomViewModel("после приёма пищи")).Add(new SymptomViewModel("шатается зуб"));

            var throat = new SymptomViewModel("горло") { IsNonCheckable = true };
            throat.Add(new SymptomViewModel("боль в горле")
                    .Add(new SymptomViewModel("при глотании"))
                    .Add(new SymptomViewModel("после разговора")))
                .Add(new SymptomViewModel("красное"));

            SymptomViewModel root = (new SymptomViewModel("root") { IsNonCheckable = true })
                .Add((new SymptomViewModel("голова") { IsNonCheckable = true }).Add(throat)
                        .Add(teeth)
                        .Add(new SymptomViewModel("уши"))
                ).Add(new SymptomViewModel("ноги") { IsNonCheckable = true });

            root.Initialize();

            return new List<SymptomViewModel>(root.Children);
        }

        private static List<DiagnosisViewModel> _Diagnoses;

        public static List<DiagnosisViewModel> Diagnoses
        {
            get
            {
                return _Diagnoses ?? (_Diagnoses = CreateDiagnoses());
            }
        }

        private static List<DiagnosisViewModel> CreateDiagnoses()
        {
            DiagnosisViewModel root = new DiagnosisViewModel("root")
                .Add(new DiagnosisViewModel("Некоторые инфекционные и паразитарные болезни")
                    .Add(new DiagnosisViewModel("Кишечные инфекции")
                        .Add(new DiagnosisViewModel("Холера")
                            .Add(new DiagnosisViewModel("Холера, вызванная вибрионом 01, биовар cholerae"))
                            .Add(new DiagnosisViewModel("Холера, вызванная вибрионом 01, биовар cholerae")))
                        .Add(new DiagnosisViewModel("Другие протозойные кишечные болезни")
                            .Add(new DiagnosisViewModel("Балантидиаз"))
                            .Add(new DiagnosisViewModel("Жиардиаз (лямблиоз)"))
                            .Add(new DiagnosisViewModel("Протозойная кишечная болезнь неуточненная")))
                        )
                    )
                ;

            root.Initialize();

            return new List<DiagnosisViewModel>(root.Children);
        }
    }
}