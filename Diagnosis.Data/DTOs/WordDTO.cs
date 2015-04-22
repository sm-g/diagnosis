using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

namespace Diagnosis.Data.DTOs
{
    [DataContract]
    public class WordDTO
    {
        [DataMember]
        public string Title { get; set; }
    }
}
