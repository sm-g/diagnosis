﻿using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class HealthRecordRepository : ModelRepository<HealthRecord>, IHealthRecordRepository
    {
    }
}