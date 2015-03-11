using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    /// <summary>
    /// Уверенность в наличии признака - элемента записи.
    /// </summary>
    public enum Confidence
    {
        /// <summary>
        /// Наличиствует
        /// </summary>
        Present = 0,
        /// <summary>
        /// Неопределнность
        /// </summary>
        Notsure,
        /// <summary>
        /// Отсутствует
        /// </summary>
        Absent
    }
}
