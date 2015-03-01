using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace Diagnosis.Client.App.Themes
{
    /// <summary>
    /// Provides helper methods to manage resources in WPF.
    /// </summary>
    public static class ResourceHelper
    {
        /// <summary>
        /// Gets the pack URI from a local resource path.
        /// </summary>
        /// <param name="resourcePath">The local resource path.</param>
        /// <param name="resourceAssembly">The assembly containing the resource.</param>
        /// <returns>The pack uri.</returns>
        public static Uri GetPackUri(this string resourcePath, Assembly resourceAssembly)
        {
            return GetPackUri(resourcePath, Application.ResourceAssembly.GetName().Name);
        }
        /// <summary>
        /// Gets the pack URI from a local resource path.
        /// </summary>
        /// <param name="resourcePath">The local resource path.</param>
        /// <returns>The pack uri.</returns>
        public static Uri GetPackUri(this string resourcePath)
        {
            return GetPackUri(resourcePath, Application.ResourceAssembly);
        }
        /// <summary>
        /// Gets the pack URI from a local resource path.
        /// </summary>
        public static Uri GetPackUri(this string resourcePath, string resourceAssemblyName)
        {
            return new Uri("pack://application:,,,/" + resourceAssemblyName + ";component/" + resourcePath);
        }
    }
}
