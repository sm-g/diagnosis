using Diagnosis.Common.Presentation.Converters;
using Diagnosis.Models;
using System;
using System.Globalization;
using System.Linq;

namespace Diagnosis.App.Converters
{
    public class ScopeToLabel : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var scope = (HealthRecordQueryAndScope)value;
            switch (scope)
            {
                case HealthRecordQueryAndScope.HealthRecord:
                    return "в записи";

                case HealthRecordQueryAndScope.Appointment:
                    return "в осмотре";

                case HealthRecordQueryAndScope.Course:
                    return "в курсе";

                case HealthRecordQueryAndScope.Patient:
                    return "у пациента";

                default:
                    return null;
            }
        }
    }
}