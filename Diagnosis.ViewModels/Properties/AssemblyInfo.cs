using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Diagnosis.ViewModels")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6d694dfc-b839-4ccd-bd84-9e3b14b961d2")]

[assembly: InternalsVisibleTo("Tests")]

[assembly: XmlnsPrefix("http://schemas.smg.com/diagnosis", "diag")]
[assembly: XmlnsDefinition("http://schemas.smg.com/diagnosis", "Diagnosis.ViewModels.Screens")]
[assembly: XmlnsDefinition("http://schemas.smg.com/diagnosis", "Diagnosis.ViewModels.Autocomplete")]
[assembly: XmlnsDefinition("http://schemas.smg.com/diagnosis", "Diagnosis.ViewModels.Search")]
[assembly: XmlnsDefinition("http://schemas.smg.com/diagnosis", "Diagnosis.ViewModels.Controls")]
[assembly: XmlnsDefinition("http://schemas.smg.com/diagnosis", "Diagnosis.ViewModels")]
