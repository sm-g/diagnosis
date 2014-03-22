using Diagnosis.Models;
using Diagnosis.App.ViewModels;
using System;
using System.Collections.Generic;

namespace Diagnosis.App
{
    public static class DataCreator
    {
        private static List<DiagnosisViewModel> _Diagnoses;

        private static List<SymptomViewModel> _Symptoms;

        public static List<DiagnosisViewModel> Diagnoses
        {
            get
            {
                return _Diagnoses ?? (_Diagnoses = CreateDiagnoses());
            }
        }


        public static List<SymptomViewModel> Symptoms
        {
            get
            {
                return _Symptoms ?? (_Symptoms = CreateSymptoms());
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
    }
}