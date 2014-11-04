using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Diagnosis.Data.Queries;

namespace Diagnosis.App.Converters
{
    public class ScopeToLabel : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var scope = (HealthRecordQuery.AndScopes)value;
            switch (scope)
            {
                case HealthRecordQuery.AndScopes.HealthRecord:
                    return "в записи";
                case HealthRecordQuery.AndScopes.Appointment:
                    return "в осмотре";
                case HealthRecordQuery.AndScopes.Course:
                    return "в курсе";
                case HealthRecordQuery.AndScopes.Patient:
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
