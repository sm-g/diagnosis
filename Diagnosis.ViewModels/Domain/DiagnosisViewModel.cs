﻿using EventAggregator;
using System.Diagnostics.Contracts;
using Diagnosis.App.Messaging;
using System;

namespace Diagnosis.ViewModels
{
    public class DiagnosisViewModel : HierarchicalBase<DiagnosisViewModel>
    {
        internal readonly Diagnosis.Models.Diagnosis diagnosis;
        EventHandler diagnosesRootChanged;

        private PopupSearch<DiagnosisViewModel> _search;

        public Editable Editable { get; private set; }

        public string Name
        {
            get
            {
                return diagnosis.Title;
            }
            set
            {
                if (diagnosis.Title != value)
                {
                    diagnosis.Title = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string SearchText
        {
            get
            {
                return Code + ' ' + Name;
            }
        }

        public string Code
        {
            get
            {
                return diagnosis.Code;
            }
            set
            {
                if (diagnosis.Code != value)
                {
                    diagnosis.Code = value;
                    OnPropertyChanged("Code");
                }
            }
        }

        public PopupSearch<DiagnosisViewModel> Search
        {
            get
            {
                return _search ?? (_search = CreateSearch());
            }
            set
            {
                if (_search != value)
                {
                    OnPropertyChanged("Search");
                }
            }
        }

        public void Unsubscribe()
        {
            diagnosesRootChanged -= RootChanged;
            ChildrenChanged -= DiagnosisViewModel_ChildrenChanged;
        }

        public PopupSearch<DiagnosisViewModel> CreateSearch()
        {
            if (_search != null)
            {
                _search.ResultItemSelected -= _search_ResultItemSelected;
            }
            var search = new PopupSearch<DiagnosisViewModel>(new DiagnosisSearcher(this, new HierarchicalSearchSettings()));
            search.ResultItemSelected += _search_ResultItemSelected;
            return search;
        }
        public DiagnosisViewModel(Diagnosis.Models.Diagnosis d, EventHandler diagnosesRootChanged)
        {
            Contract.Requires(d != null);
            this.diagnosis = d;
            this.diagnosesRootChanged = diagnosesRootChanged;

            Editable = new Editable(diagnosis);

            ChildrenChanged += DiagnosisViewModel_ChildrenChanged;
            diagnosesRootChanged += RootChanged;
        }

        void RootChanged(object s, EventArgs e)
        {
            Search = CreateSearch();
        }

        private void _search_ResultItemSelected(object sender, System.EventArgs e)
        {
            this.AddIfNotExists(Search.SelectedItem, Search.searcher.AllChildren);
            Search.SelectedItem.IsChecked = true;
            Search.Clear();
        }

        private void DiagnosisViewModel_ChildrenChanged(object sender, HierarchicalEventAgrs<DiagnosisViewModel> e)
        {
            IsNonCheckable = !IsTerminal;
        }

        public override string ToString()
        {
            return diagnosis.ToString();
        }

    }
}