using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Diagnosis.Data.Queries;
using Diagnosis.Models;

namespace Diagnosis.App.Converters
{
    public class ScopeToLabel : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
