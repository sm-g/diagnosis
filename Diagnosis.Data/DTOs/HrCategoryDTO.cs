using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Diagnosis.Data.DTOs
{
    [DataContract]
    public class HrCategoryDTO
    {
        [DataMember]
        public string Title { get; set; }

    }
}
