﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Data.Repositories;
using System.Diagnostics.Contracts;
using Diagnosis.Models;
using Diagnosis.Core;

namespace Diagnosis.App.ViewModels
{
    public class HrSearcher
    {
        public IEnumerable<HealthRecord> Search(HrSearchOptions options)
        {
            Contract.Requires(options != null);

            var repo = new HealthRecordRepository();
            IEnumerable<HealthRecord> hrs;

            if (options.Words.Count() > 0)
                if (options.AnyWord)
                {
                    hrs = repo.GetByWords(options.Words.Select(w => w.word));
                }
                else
                {
                    hrs = repo.GetWithAllWords(options.Words.Select(w => w.word));
                }
            else
            {
                hrs = repo.GetAll();
            }

            hrs = hrs.Where(hr => TestHrDate(hr, options));

            hrs = hrs.Where(hr =>
               (options.AppointmentDateLt.HasValue ? hr.Appointment.DateAndTime <= options.AppointmentDateLt.Value : true) &&
               (options.AppointmentDateGt.HasValue ? hr.Appointment.DateAndTime >= options.AppointmentDateGt.Value : true)
            );

            if (options.Categories.Count() > 0)
            {
                hrs = hrs.Where(hr =>
                    options.Categories.Any(cat => cat.category == hr.Category));
            }

            if (options.Comment != null)
            {
                hrs = hrs.Where(hr => hr.Comment != null && hr.Comment.ToLower().Contains(options.Comment.ToLower()));
            }

            return hrs;
        }

        bool TestHrDate(HealthRecord hr, HrSearchOptions options)
        {
            if (options.HealthRecordOffsetLt.IsEmpty || options.HealthRecordOffsetGt.IsEmpty)
                return true; // условия поиска не заданы
            var hrDateOffset = new DateOffset(hr.FromYear, hr.FromMonth, hr.FromDay, () => hr.Appointment.DateAndTime);
            if (hrDateOffset.Unit == DateUnits.Week)
            {
                ;
            }

            var hrDateLtThat = new DateOffset(options.HealthRecordOffsetLt, () => hr.Appointment.DateAndTime);
            var grDateGtThat = new DateOffset(options.HealthRecordOffsetGt, () => hr.Appointment.DateAndTime);

            return !hrDateOffset.IsEmpty &&
                   hrDateOffset <= hrDateLtThat &&
                   hrDateOffset >= grDateGtThat;
        }
    }
}
