﻿using Diagnosis.App.Messaging;
using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class HrEditorViewModel : ViewModelBase
    {
        private static AutoCompleteBase<WordViewModel> _autoCompleteStatic;
        private PopupSearch<DiagnosisViewModel> _diagnosisSearch;
        private List<EventMessageHandler> msgHandlers;

        private HealthRecordViewModel _hr;
        public HealthRecordViewModel HealthRecord
        {
            get
            {
                return _hr;
            }
            set
            {
                if (_hr != value)
                {
                    _hr = value;

                    CreateAutoComplete();
                    UpdateDiagnosisQueryCode();
                    OnPropertyChanged(() => HealthRecord);
                }
            }
        }

        #region AutoComplete

        public AutoCompleteBase<WordViewModel> AutoComplete { get { return _autoCompleteStatic; } }

        private void CreateAutoComplete()
        {
            if (_autoCompleteStatic != null)
            {
                ((INotifyCollectionChanged)_autoCompleteStatic.Items).CollectionChanged -= AutoCompleteItems_CollectionChanged;
            }

            IEnumerable<WordViewModel> initialWords = HealthRecord.Symptom != null ? HealthRecord.Symptom.Words : null;
            _autoCompleteStatic = new WordCompositeAutoComplete(
                   QuerySeparator.Default,
                   new SimpleSearcherSettings() { AllChildren = true },
                   initialWords);

            ((INotifyCollectionChanged)_autoCompleteStatic.Items).CollectionChanged += AutoCompleteItems_CollectionChanged;

            OnPropertyChanged(() => AutoComplete);
        }

        private void AutoCompleteItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HashSet<WordViewModel> words;

            if (HealthRecord.Symptom != null)
                words = new HashSet<WordViewModel>(HealthRecord.Symptom.Words);
            else
                words = new HashSet<WordViewModel>();

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (WordViewModel item in e.NewItems)
                {
                    words.Add(item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (WordViewModel item in e.OldItems)
                {
                    words.Remove(item);
                }
            }

            HealthRecord.Symptom = EntityManagers.SymptomsManager.Create(words);
        }

        #endregion AutoComplete

        public PopupSearch<DiagnosisViewModel> DiagnosisSearch
        {
            get
            {
                return _diagnosisSearch;
            }
            private set
            {
                if (_diagnosisSearch != value)
                {
                    _diagnosisSearch = value;
                    OnPropertyChanged("DiagnosisSearch");
                }
            }
        }

        public bool ShowIcdDisease
        {
            get
            {
                var b = EntityManagers.DoctorsManager.CurrentDoctor.doctor.DoctorSettings.HasFlag(DoctorSettings.ShowIcdDisease);
                return b;
            }
        }

        public HrEditorViewModel()
        {
            Subscribe();
            CreateDiagnosisSearch();
        }

        public void UnsubscribeCheckedChanges()
        {
            foreach (var h in msgHandlers)
            {
                h.Dispose();
            }
        }

        private void CreateDiagnosisSearch()
        {
            if (DiagnosisSearch != null)
            {
                DiagnosisSearch.Cleared -= DiagnosisSearch_Cleared;
            }
            DiagnosisSearch = new PopupSearch<DiagnosisViewModel>(
                   EntityManagers.DiagnosisManager.RootFiltratingSearcher,
                   onSelected: (dia) => { dia.IsChecked = true; });

            DiagnosisSearch.Cleared += DiagnosisSearch_Cleared;

            UpdateDiagnosisQueryCode();
        }

        private void DiagnosisSearch_Cleared(object sender, EventArgs e)
        {
            HealthRecord.Diagnosis = null;
            // DiagnosisSearch.Query already empty
        }

        private void UpdateDiagnosisQueryCode()
        {
            if (DiagnosisSearch != null && HealthRecord != null)
            {
                DiagnosisSearch.UpdateResultsOnQueryChanges = false;

                if (HealthRecord.Diagnosis != null)
                    DiagnosisSearch.Query = HealthRecord.Diagnosis.Code;
                else
                    DiagnosisSearch.Query = "";

                DiagnosisSearch.UpdateResultsOnQueryChanges = true;
            }
        }

        #region Event handlers

        private void Subscribe()
        {
            EntityManagers.DiagnosisManager.RootChanged += (s, e) =>
            {
                CreateDiagnosisSearch();
            };
            this.Subscribe((int)EventID.SettingsSaved, (e) =>
            {
                OnPropertyChanged(() => ShowIcdDisease);
            });
        }

        private void SubscribeToCheckedChanges()
        {
            msgHandlers = new List<EventMessageHandler>()
            {
                this.Subscribe((int)EventID.DiagnosisCheckedChanged, (e) =>
                {
                    if (HealthRecord.IsSelected)
                    {
                        UpdateDiagnosisQueryCode();
                    }
                })
            };
        }

        #endregion Event handlers

        public override string ToString()
        {
            return string.Format("editor {0}", HealthRecord);
        }
    }
}