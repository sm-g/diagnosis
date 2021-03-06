﻿using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class HrsSearchResultViewModel : AbstractSearchResultViewModel
    {
        private readonly SearchOptions options;

        public HrsSearchResultViewModel(IEnumerable<HealthRecord> hrs, SearchOptions options)
        {
            this.options = options;

            Patients = new ObservableCollection<IResultItem>();

            HrsResultItemViewModel.MakeFrom(hrs).ForAll(x =>
            {
                x.ForBranch(rvm => rvm.IsExpanded = true);
                Patients.Add(x);
            });

            Statistic = new HrsStatistic(hrs);
        }

        /// <summary>
        /// Опции, которыми получен результат.
        /// </summary>
        public override object QuerySource
        {
            get { return options; }
        }

        protected override void RemoveDeleted(IHrsHolder h)
        {
            var hrsResultItem = Patients.Cast<HrsResultItemViewModel>().FindHolderKeeperOf(h);
            if (hrsResultItem != null)
            {
                if (hrsResultItem.IsRoot)
                    Patients.Remove(hrsResultItem);
                else
                    hrsResultItem.Remove();
            }
        }

        protected override void Export()
        {
            new Exporter().ExportToXlsx(Statistic as HrsStatistic);
        }
    }
}