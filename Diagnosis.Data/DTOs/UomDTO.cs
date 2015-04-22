using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

namespace Diagnosis.Data.DTOs
{
    [DataContract]
    public class UomDTO
    {
        [DataMember]
        public string Abbr { get; set; }
        [DataMember]
        public double Factor { get; set; }
        [DataMember]
        public UomTypeDTO UomType { get; set; }

    }
}
