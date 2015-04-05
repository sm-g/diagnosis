// http://www.codeproject.com/Articles/29495/Binding-and-Using-Friendly-Enums-in-WPF

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Diagnosis.Common.Attributes
{
    /// <summary>
    /// Attribute for localization.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class LocalizableDescriptionAttribute : DescriptionAttribute
    {
        public static Type ResourcesType;
        #region Public methods.
        // ------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="LocalizableDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="resourcesType">Type of the resources.</param>
        public LocalizableDescriptionAttribute
        (string description, Type resourcesType)
            : base(description)
        {
            _resourcesType = resourcesType;
        }
        public LocalizableDescriptionAttribute
       (string description)
            : base(description)
        {
        }
        #endregion

        #region Public properties.

        /// <summary>
        /// Get the string value from the resources.
        /// </summary>
        /// <value></value>
        /// <returns>The description stored in this attribute.</returns>
        public override string Description
        {
            get
            {
                if (!_isLocalized && !(_resourcesType == null && ResourcesType == null))
                {
                    var type = _resourcesType ?? ResourcesType;
                    ResourceManager resMan =
                         type.InvokeMember(
                         @"ResourceManager",
                         BindingFlags.GetProperty | BindingFlags.Static |
                         BindingFlags.Public | BindingFlags.NonPublic,
                         null,
                         null,
                         new object[] { }) as ResourceManager;

                    CultureInfo culture =
                         type.InvokeMember(
                         @"Culture",
                         BindingFlags.GetProperty | BindingFlags.Static |
                         BindingFlags.Public | BindingFlags.NonPublic,
                         null,
                         null,
                         new object[] { }) as CultureInfo;

                    _isLocalized = true;

                    if (resMan != null)
                    {
                        DescriptionValue =
                             resMan.GetString(DescriptionValue, culture);
                    }
                }

                return DescriptionValue;
            }
        }

        #endregion

        #region Private variables.

        private readonly Type _resourcesType;
        private bool _isLocalized;

        #endregion
    }
}
