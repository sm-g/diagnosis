﻿using Diagnosis.App.Messaging;
using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Diagnosis.ViewModels
{
    public class HrEditorViewModel : ViewModelBase
    {
        private static AutoCompleteBase<WordViewModel> _autoCompleteStatic;
        private PopupSearch<DiagnosisViewModel> _diagnosisSearch;

        private HealthRecordViewModel _hr;

        #region HealthRecord

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
                    if (_hr != null)
                    {
                        _hr.Editable.PropertyChanged -= hr_Editable_PropertyChanged;
                    }

                    _hr = value;

                    if (value != null)
                    {
                        CreateAutoComplete();
                        UpdateDiagnosisQueryCode();
                        _hr.Editable.PropertyChanged += hr_Editable_PropertyChanged;
                    }
                    OnPropertyChanged("HealthRecord");
                    OnPropertyChanged("IsActive");
                }
            }
        }

        public bool IsActive
        {
            get
            {
                return HealthRecord != null && HealthRecord.Editable.IsEditorActive;
            }
        }

        private void hr_Editable_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEditorActive")
            {
                OnPropertyChanged("IsActive");
            }
        }

        #endregion HealthRecord

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
                   new HierarchicalSearchSettings() { AllChildren = true },
                   initialWords);

            ((INotifyCollectionChanged)_autoCompleteStatic.Items).CollectionChanged += AutoCompleteItems_CollectionChanged;

            OnPropertyChanged("AutoComplete");
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

            HealthRecord.Symptom = EntityProducers.SymptomsProducer.Create(words);
        }

        #endregion AutoComplete

        #region Diagnosis search

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

        private void CreateDiagnosisSearch()
        {
            if (DiagnosisSearch != null)
            {
                DiagnosisSearch.Cleared -= DiagnosisSearch_Cleared;
                DiagnosisSearch.ResultItemSelected -= DiagnosisSearch_ResultItemSelected;
            }
            DiagnosisSearch = new PopupSearch<DiagnosisViewModel>(
                   EntityProducers.DiagnosisProducer.RootFiltratingSearcher
                   );

            DiagnosisSearch.Cleared += DiagnosisSearch_Cleared;
            DiagnosisSearch.ResultItemSelected += DiagnosisSearch_ResultItemSelected;

            UpdateDiagnosisQueryCode();
        }

        private void DiagnosisSearch_ResultItemSelected(object sender, VmBaseEventArgs e)
        {
            HealthRecord.Diagnosis = e.vm as DiagnosisViewModel;
            if (HealthRecord != null)
            {
                UpdateDiagnosisQueryCode();
            }
        }

        private void DiagnosisSearch_Cleared(object sender, EventArgs e)
        {
            HealthRecord.Diagnosis = null;
            // DiagnosisSearch.Query already empty
        }

        #endregion Diagnosis search

        public bool ShowIcdDisease
        {
            get
            {
                var b = EntityProducers.DoctorsProducer.CurrentDoctor.doctor.DoctorSettings.HasFlag(DoctorSettings.ShowIcdDisease);
                return b;
            }
        }

        public HrEditorViewModel()
        {
            Subscribe();
            CreateDiagnosisSearch();
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

        private void Subscribe()
        {
            EntityProducers.DiagnosisProducer.RootChanged += (s, e) =>
            {
                CreateDiagnosisSearch();
            };
            this.Subscribe((int)EventID.SettingsSaved, (e) =>
            {
                OnPropertyChanged("ShowIcdDisease");
            });
        }

        public override string ToString()
        {
            return string.Format("editor {0}", HealthRecord);
        }
    }
}