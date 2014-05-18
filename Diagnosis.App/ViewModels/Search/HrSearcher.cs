using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Data.Repositories;
using System.Diagnostics.Contracts;
using Diagnosis.Models;

namespace Diagnosis.App.ViewModels
{
    public class HrSearcher
    {
        public IEnumerable<HealthRecord> Search(HrSearchOptions options)
        {
            Contract.Requires(options != null);

            return new HealthRecordRepository().GetByWords(options.Words.Select(w => w.word)).Where(hr =>
                hr.DateOffset.Offset == null ||
                    (options.HealthRecordFromDateLt.Offset == null || hr.DateOffset < options.HealthRecordFromDateLt) &&
                    (options.HealthRecordFromDateGt.Offset == null || hr.DateOffset < options.HealthRecordFromDateGt)
            );
        }
    }
}
