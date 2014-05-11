using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.App.ViewModels
{
    public class SearchOptions
    {
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
        public IEnumerable<CategoryViewModel> Categories { get; set; }
        /// <summary>
        /// Нижняя грань даты приема
        /// </summary>
        public DateOffset AppointmentDateGt { get; set; }
        /// <summary>
        /// Верхняя грань даты приема
        /// </summary>
        public DateOffset AppointmentDateLt { get; set; }
        /// <summary>
        /// Нижняя грань давности симптома
        /// </summary>
        public DateOffset HealthRecordFromDateGt { get; set; }
        /// <summary>
        /// Верхняя грань давности симптома
        /// </summary>
        public DateOffset HealthRecordFromDateLt { get; set; }
    }
}
