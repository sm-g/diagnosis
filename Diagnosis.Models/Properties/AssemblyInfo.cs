﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Diagnosis.Models")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("22d9921a-7b02-4342-8ada-a7f7c092746b")]

[assembly: InternalsVisibleTo("Diagnosis.Data")]
[assembly: InternalsVisibleTo("Diagnosis.Tests")]
[assembly: InternalsVisibleTo("Diagnosis.Data.Tests")]
[assembly: InternalsVisibleTo("Diagnosis.Models.Tests")]
[assembly: InternalsVisibleTo("Diagnosis.ViewModels.Tests")]

[assembly: XmlnsPrefix("http://schemas.smg.com/diagnosis", "diag")]
[assembly: XmlnsDefinition("http://schemas.smg.com/diagnosis", "Diagnosis.Models")]
[assembly: XmlnsDefinition("http://schemas.smg.com/diagnosis", "Diagnosis.Models.Enums")]