using Diagnosis.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public enum MeasureOperator
    {
        [LocalizableDescription(@"Operator_GtEq")]
        GreaterOrEqual,
        [LocalizableDescription(@"Operator_Gt")]
        Greater,
        [LocalizableDescription(@"Operator_Eq")]
        Equal,
        [LocalizableDescription(@"Operator_Lt")]
        Less,
        [LocalizableDescription(@"Operator_LtEq")]
        LessOrEqual
    }
}
