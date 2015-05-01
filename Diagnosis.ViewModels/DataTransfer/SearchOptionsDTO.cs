using Diagnosis.Data.DTOs;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Diagnosis.ViewModels.Search
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
        public QueryGroupOperator GroupOperator { get; set; }

        [DataMember]
        public int MinAny { get; set; }

        [DataMember]
        public SearchScope SearchScope { get; set; }

        [DataMember]
        public List<SearchOptionsDTO> Children { get; private set; }
    }
}