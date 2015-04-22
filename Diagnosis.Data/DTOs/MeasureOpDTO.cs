﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Diagnosis.Models;

namespace Diagnosis.Data.DTOs
{
    [DataContract]
    public class MeasureOpDTO
    {
        [DataMember]
        public double DbValue { get; set; }
        [DataMember]
        public UomDTO Uom { get; set; }
        [DataMember]
        public WordDTO Word { get; set; }
        [DataMember]
        public MeasureOp Operator { get; set; }

    }
}
