using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Diagnosis.Data.Queries;
using Diagnosis.Common.Types;

namespace Diagnosis.ViewModels.Search
{
    public class HrSearchOptions
    {
        private DateTime? _appointmentDateGt;
        private DateTime? _appointmentDateLt;
        DateOffset _hrOffsetGt;
        DateOffset _hrOffsetLt;

        #region Options

        /// <summary>
        /// Записи со всеми словами
        /// </summary>
        public IEnumerable<Word> WordsAll { get; set; }
        /// <summary>
        /// И любым словом из
        /// </summary>
        public IEnumerable<Word> WordsAny { get; set; }
        /// <summary>
        /// B ни одного слова из.
        /// </summary>
        public IEnumerable<Word> WordsNot { get; set; }
        /// <summary>
        /// Записи со всеми измерениями
        /// </summary>
        public IEnumerable<MeasureOp> MeasuresAll { get; set; }
        /// <summary>
        /// И любым из
        /// </summary>
        public IEnumerable<MeasureOp> MeasuresAny { get; set; }

        /// <summary>
        /// В области должны быть все слова из запроса.
        /// </summary>
        public bool AllWords { get; set; }

        /// <summary>
        /// Область поиска всех слов.
        /// </summary>
        public HealthRecordQueryAndScope QueryScope { get; set; }

        /// <summary>
        /// Категория. Если несколько, то любая их них.
        /// </summary>
        public IEnumerable<HrCategory> Categories { get; set; }

        /// <summary>
        /// Нижняя грань даты осмотра
        /// </summary>
        public DateTime? AppointmentDateGt
        {
            get { return _appointmentDateGt; }
            set
            {
                if (AppointmentDateLt.HasValue && value.HasValue &&
                    value.Value >= AppointmentDateLt.Value)
                {
                    _appointmentDateGt = _appointmentDateLt;
                    _appointmentDateLt = value;
                }
                else
                {
                    _appointmentDateGt = value;
                }
            }
        }

        /// <summary>
        /// Верхняя грань даты осмотра
        /// </summary>
        public DateTime? AppointmentDateLt
        {
            get { return _appointmentDateLt; }
            set
            {
                if (AppointmentDateGt.HasValue && value.HasValue &&
                    value.Value <= AppointmentDateGt.Value)
                {
                    _appointmentDateLt = _appointmentDateGt;
                    _appointmentDateGt = value;
                }
                else
                {
                    _appointmentDateLt = value;
                }
            }
        }

        // границы интервала давности могут быть введены в любом порядке
        // HrDateOffsetLower = 5 дней
        // HrDateOffsetUpper = 1 день
        // тогда HrDateGt = HrDateOffsetLower
        //       HrDateLt = HrDateOffsetUpper
        // ищем X = 3 дня: HrDateGt < X < HrDateLt

        /// <summary>
        /// Нижняя грань давности записи
        /// </summary>
        public DateOffset HealthRecordOffsetGt
        {
            get { return _hrOffsetGt; }
            set
            {
                if (HealthRecordOffsetLt != null && !value.IsEmpty &&
                    value >= HealthRecordOffsetLt)
                {
                    _hrOffsetGt = _hrOffsetLt;
                    _hrOffsetLt = value;
                }
                else
                {
                    _hrOffsetGt = value;
                }
            }
        }

        /// <summary>
        /// Верхняя грань давности записи
        /// </summary>
        public DateOffset HealthRecordOffsetLt
        {
            get { return _hrOffsetLt; }
            set
            {
                if (HealthRecordOffsetGt != null && !value.IsEmpty &&
                    value <= HealthRecordOffsetGt)
                {
                    _hrOffsetLt = _hrOffsetGt;
                    _hrOffsetGt = value;
                }
                else
                {
                    _hrOffsetLt = value;
                }
            }
        }



        #endregion

        public bool AppDateVisible
        {
            get
            {
                return AppointmentDateLt != null || AppointmentDateGt != null;
            }
        }

        public bool HrDateVisible
        {
            get
            {
                return !HealthRecordOffsetLt.IsEmpty && !HealthRecordOffsetGt.IsEmpty;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!AppointmentDateGt.HasValue || !AppointmentDateLt.HasValue ||
                AppointmentDateGt <= AppointmentDateLt);

            Contract.Invariant(HealthRecordOffsetGt == null || HealthRecordOffsetLt == null || HealthRecordOffsetGt.IsEmpty || HealthRecordOffsetLt.IsEmpty ||
                HealthRecordOffsetGt <= HealthRecordOffsetLt);
        }
    }
}