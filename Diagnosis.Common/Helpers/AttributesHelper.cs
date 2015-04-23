using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Diagnosis.Common
{
    public class AttributesHelper
    {
        /// <summary>
        /// Description, если значение помечено DescriptionAttribute,
        /// иначе ToString.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumDescription(object enumValue)
        {
            // from http://www.codeproject.com/Articles/29495/Binding-and-Using-Friendly-Enums-in-WPF
            string str = enumValue.ToString();
            FieldInfo fi = enumValue.GetType().GetField(str);
            if (fi != null)
            {
                var attribs = fi.GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>().ToArray();
                if (null != attribs && attribs.FirstOrDefault() != null && !String.IsNullOrEmpty(attribs[0].Description))
                {
                    str = attribs[0].Description;
                }
            }
            return str;
        }
    }
}
