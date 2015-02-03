using System.IO;
using System.Text;
using System.Windows.Markup;
using System.Xml;

namespace Diagnosis.Common.Controls
{
    public static class XamlExtensions
    {
        /// <summary>
        /// Clones xaml object preserving bindings
        /// from http://stackoverflow.com/a/4973369/3009578
        /// </summary>
        public static T XamlClone<T>(this T original)
            where T : class
        {
            if (original == null)
                return null;

            var sb = new StringBuilder();
            var writer = XmlWriter.Create(sb, new XmlWriterSettings
            {
                Indent = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
            });

            var mgr = new XamlDesignerSerializationManager(writer)
            {
                XamlWriterMode = XamlWriterMode.Expression
            };

            XamlWriter.Save(original, mgr);

            object clone;
            using (var sr = new StringReader(sb.ToString()))
            {
                var r = XmlReader.Create(sr);
                clone = XamlReader.Load(r);
            }

            if (clone is T)
                return (T)clone;
            else
                return null;
        }
    }
}