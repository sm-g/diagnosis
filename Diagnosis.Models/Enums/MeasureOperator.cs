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
        LessOrEqual,
        [LocalizableDescription(@"Operator_Between")]
        Between
    }
    public static class MeasureOperatorExtensions
    {
        public static bool IsBinary(this MeasureOperator op)
        {
            return op == MeasureOperator.Between;
        }
        public static string ToStr(this MeasureOperator op)
        {
            switch (op)
            {
                case MeasureOperator.GreaterOrEqual:
                    return "≥";
                case MeasureOperator.Greater:
                    return ">";
                case MeasureOperator.Equal:
                    return "=";
                case MeasureOperator.Less:
                    return "<";
                case MeasureOperator.LessOrEqual:
                    return "≤";
                case MeasureOperator.Between:
                    return "—";
                default:
                    return op.ToString();
            }
        }
    }
}
