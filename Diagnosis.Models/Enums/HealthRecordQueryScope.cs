using Diagnosis.Common.Attributes;
using Diagnosis.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public enum HealthRecordQueryAndScope
    {
        [LocalizableDescription(@"HealthRecordQueryAndScope_HealthRecord")]
        HealthRecord,
        [LocalizableDescription(@"HealthRecordQueryAndScope_Appointment")]
        Appointment,
        [LocalizableDescription(@"HealthRecordQueryAndScope_Course")]
        Course,
        [LocalizableDescription(@"HealthRecordQueryAndScope_Patient")]
        Patient
    }

    public enum SearchScope
    {
        [LocalizableDescription(@"SearchScope_HealthRecord")]
        HealthRecord,
        [LocalizableDescription(@"SearchScope_Holder")]
        Holder,
        [LocalizableDescription(@"SearchScope_Patient")]
        Patient
    }
}
