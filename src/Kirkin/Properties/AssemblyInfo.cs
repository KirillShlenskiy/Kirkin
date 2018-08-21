using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyDescription("Core Kirkin component library")]
[assembly: AssemblyCopyright("Copyright © Kirill Shlenskiy 2018")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
#if !NETSTANDARD2_0
[assembly: AssemblyTitle("Kirkin")]
[assembly: AssemblyProduct("Kirkin")]
[assembly: AssemblyCompany("Kirill Shlenskiy")]
[assembly: AssemblyVersion("1.7.3")]
#endif

// Allow unit testing internal members.
[assembly: InternalsVisibleTo("Kirkin.Experimental")]
[assembly: InternalsVisibleTo("Kirkin.Tests")]