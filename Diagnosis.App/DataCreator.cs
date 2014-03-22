using Diagnosis.Models;
using Diagnosis.App.ViewModels;
using System;
using System.Collections.Generic;

namespace Diagnosis.App
{
    public static class DataCreator
    {
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