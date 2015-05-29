using AutoMapper;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Data.DTOs
{
    public static class ModelDtosMapper
    {
        public static void Map()
        {
            Mapper.CreateMap<Word, WordDTO>();
            Mapper.CreateMap<Confindencable<Word>, ConfWordDTO>()
                .ForMember(d => d.Title, o => o.MapFrom(s => s.HIO.Title))
                .ForMember(d => d.Confidence, o => o.MapFrom(s => s.Confidence.ToString()));
            Mapper.CreateMap<MeasureOp, MeasureOpDTO>()
                .ForMember(d => d.Value, o => o.MapFrom(s => s.Value))
                .ForMember(d => d.RightValue, o => o.MapFrom(s => s.RightDbValue))
                .ForMember(d => d.Operator, o => o.MapFrom(s => s.Operator.ToString()))
                .ForMember(d => d.UomDescr, o => o.MapFrom(s => s.Uom.Description))
                .ForMember(d => d.Abbr, o => o.MapFrom(s => s.Uom.Abbr))
                .ForMember(d => d.UomTypeTitle, o => o.MapFrom(s => s.Uom.Type.Title))
                .ForMember(d => d.Word, o => o.MapFrom(s => s.Word));
            Mapper.CreateMap(typeof(HrCategory), typeof(HrCategoryDTO));
            Mapper.CreateMap(typeof(SearchOptions), typeof(SearchOptionsDTO));
        }
    }
}
