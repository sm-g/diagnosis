using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using Diagnosis.Models;

namespace Diagnosis.Data.DTOs
{
    [DataContract]
    public class ConfWordDTO
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public Confidence Confidence { get; set; }
    }
}
