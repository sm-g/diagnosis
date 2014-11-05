﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class SpecialityIcdBlocks : EntityBase
    {
        public virtual Speciality Speciality { get; protected set; }
        public virtual IcdBlock IcdBlock { get; protected set; }

        protected SpecialityIcdBlocks() { }
    }
}