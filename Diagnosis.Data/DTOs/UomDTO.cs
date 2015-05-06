using Diagnosis.Models;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Diagnosis.Data.DTOs
{
    [DataContract]
    public class UomDTO
    {
        [DataMember]
        public string Abbr { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public UomTypeDTO Type { get; set; }

        // TODO use RelHelper
        public override bool Equals(object obj)
        {
            var dto = obj as UomDTO;
            var uom = obj as Uom;
            if (dto != null)
            {
                return
                    Abbr == dto.Abbr &&
                    Description == dto.Description &&
                    (Type == null && dto.Type == null ||
                    Type.Title == dto.Type.Title);
            }

            if (uom != null)
            {
                return
                    uom.Abbr == Abbr &&
                    uom.Description == Description &&
                    (uom.Type == null && Type == null ||
                    uom.Type.Title == Type.Title);
            }
            return false;
        }
    }
}