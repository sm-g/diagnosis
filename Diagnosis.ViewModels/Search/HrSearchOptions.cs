using Diagnosis.Core;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels
{
    public class HrSearchOptions
    {
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!AppointmentDateGt.HasValue || !AppointmentDateLt.HasValue ||
                AppointmentDateGt <= AppointmentDateLt);

            Contract.Invariant(HealthRecordOffsetGt == null || HealthRecordOffsetLt == null || HealthRecordOffsetGt.IsEmpty || HealthRecordOffsetLt.IsEmpty ||
                HealthRecordOffsetGt <= HealthRecordOffsetLt);
        }

        private DateTime? _appointmentDateGt;
        private DateTime? _appointmentDateLt;
        DateOffset _hrOffsetGt;
        DateOffset _hrOffsetLt;

        #region Options

        /// <summary>
        /// Слова, которые есть в симптоме
        /// </summary>
        public IEnumerable<WordViewModel> Words { get; set; }

        /// <summary>
        /// Достаточно ли любого слова в симптоме
        /// </summary>
        public bool AnyWord { get; set; }

        /// <summary>
        /// Категория. Если несколько, то любая их них.
        /// </summary>
        public IEnumerable<Category> Categories { get; set; }

        /// <summary>
        /// Нижняя грань даты приема
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
        /// Верхняя грань даты приема
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
        /// Нижняя грань давности симптома
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
        /// Верхняя грань давности симптома
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

        /// <summary>
        /// Часть комментария.
        /// </summary>
        public string Comment { get; set; }
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

        public bool CommentVisible
        {
            get
            {
                return !string.IsNullOrEmpty(Comment);
            }
        }
    }
}