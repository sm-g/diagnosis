﻿using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class SymptomSearch : SearchBase<SymptomViewModel>
    {
        /// <summary>
        /// Уровень (глубина), ниже которого и включая который искать симптомы.
        /// </summary>
        public int UpperLevel { get; set; }
        public bool AllChildren { get; set; }

        public SymptomSearch(SymptomViewModel parent, bool withNonCheckable = false, bool withChecked = false, bool allChildren = true, int upperLevel = 0)
            : base(withNonCheckable, withChecked)
        {
            Contract.Requires(parent != null);

            AllChildren = allChildren;
            UpperLevel = upperLevel;

            Collection = AllChildren ? parent.AllChildren : parent.Children;

            InitQuery();
        }

        protected override SymptomViewModel Add(string query)
        {
            return new SymptomViewModel(new Symptom()
                {
                    Level = UpperLevel,
                    Title = query
                });
        }

        protected override bool CheckConditions(SymptomViewModel obj)
        {
            return (WithChecked || !obj.IsChecked)
                   && (WithNonCheckable || !obj.IsNonCheckable)
                   && (UpperLevel <= obj.Level);
        }
    }
}