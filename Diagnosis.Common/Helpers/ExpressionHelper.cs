using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Diagnosis.Common
{
    public class ExpressionHelper
    {
        public static Tuple<string, T> GetPropertyNameAndValue<T>(Expression<Func<T>> propertyExpression)
        {
            var memberExpr = propertyExpression.Body as MemberExpression;
            string propertyName = memberExpr.Member.Name;
            T value = propertyExpression.Compile()(); // see http://blogs.msdn.com/b/csharpfaq/archive/2010/03/11/how-can-i-get-objects-and-property-values-from-expression-trees.aspx                   

            return new Tuple<string, T>(propertyName, value);
        }
    }
}
