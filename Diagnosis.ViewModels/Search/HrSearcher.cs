using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Diagnosis.Models;
using Diagnosis.Common;
using Diagnosis.Data.Queries;
using NHibernate;
using NHibernate.Linq;
using Diagnosis.Common.Types;

namespace Diagnosis.ViewModels.Search
{
    public class HrSearcher
    {
        public IEnumerable<HealthRecord> Search(ISession session, HrSearchOptions options)
        {
            Contract.Requires(options != null);
            Contract.Requires(session != null);

            IEnumerable<HealthRecord> hrs;

            if (options.Words.Count() > 0)
                if (options.AllWords)
                {
                    hrs = HealthRecordQuery.WithAllWords(session)(options.Words, options.QueryScope);
                }
                else
                {
                    hrs = HealthRecordQuery.WithAnyWord(session)(options.Words);
                }
            else
            {
                hrs = session.Query<HealthRecord>();
            }

            if (options.MeasuresAll.Count() > 0)
            {
                hrs = hrs.Where(x =>
                   options.MeasuresAll.All(m => x.Measures.Contains(m, new ValueComparer(m.Operator))));
            }

            if (options.Categories.Count() > 0)
            {
                hrs = hrs.Where(hr =>
                    options.Categories.Any(cat => cat.Equals(hr.Category)));
            }

            return hrs.ToList();
        }

        bool TestHrDate(HealthRecord hr, HrSearchOptions options)
        {
            if (options.HealthRecordOffsetLt.IsEmpty || options.HealthRecordOffsetGt.IsEmpty)
                return true; // условия поиска не заданы
            var hrDateOffset = new DateOffset(hr.FromYear, hr.FromMonth, hr.FromDay, () => hr.Appointment.DateAndTime);
            if (hrDateOffset.Unit == DateUnit.Week)
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
