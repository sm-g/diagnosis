﻿using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Diagnosis.Data.DTOs
{
    [DataContract]
    public class SearchOptionsDTO
    {
        [DataMember]
        public List<ConfWordDTO> CWordsAll { get; set; }

        [DataMember]
        public List<ConfWordDTO> CWordsAny { get; set; }

        [DataMember]
        public List<ConfWordDTO> CWordsNot { get; set; }

        [DataMember]
        public List<MeasureOpDTO> MeasuresAny { get; set; }

        [DataMember]
        public List<MeasureOpDTO> MeasuresAll { get; set; }

        [DataMember]
        public List<HrCategoryDTO> Categories { get; set; }

        [DataMember]
        public string GroupOperator { get; set; }

        [DataMember]
        public int MinAny { get; set; }

        [DataMember]
        public string SearchScope { get; set; }
        [DataMember]
        public bool WithConf { get; set; }


        [DataMember]
        public List<SearchOptionsDTO> Children { get; private set; }
    }
}