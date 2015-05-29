using Diagnosis.Models;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Diagnosis.Data.DTOs
{
    [DataContract]
    public class MeasureOpDTO
    {
        [DataMember]
        public double Value { get; set; }

        [DataMember]
        public double RightValue { get; set; }

        [DataMember]
        public WordDTO Word { get; set; }

        [DataMember]
        public string Operator { get; set; }

        [DataMember]
        public string Abbr { get; set; }

        [DataMember]
        public string UomDescr { get; set; }

        /// <summary>
        /// May be null.
        /// </summary>
        [DataMember]
        public string UomTypeTitle { get; set; }

        public bool UomEquals(Uom uom)
        {
            if (uom != null)
            {
                return
                    uom.Abbr == Abbr &&
                    uom.Description == UomDescr &&
                    (uom.Type == null && UomTypeTitle == null ||
                    uom.Type.Title == UomTypeTitle);
            }
            return false;
        }
    }
}