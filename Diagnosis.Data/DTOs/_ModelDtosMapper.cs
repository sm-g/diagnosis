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
            Mapper.CreateMap(typeof(Word), typeof(WordDTO));
            Mapper.CreateMap(typeof(Uom), typeof(UomDTO));
            Mapper.CreateMap(typeof(UomType), typeof(UomTypeDTO));
            Mapper.CreateMap(typeof(MeasureOp), typeof(MeasureOpDTO));
            Mapper.CreateMap(typeof(HrCategory), typeof(HrCategoryDTO));
            Mapper.CreateMap(typeof(SearchOptions), typeof(SearchOptionsDTO));
        }
    }
}
