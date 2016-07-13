﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Kirkin")]
[assembly: AssemblyDescription("Core Kirkin component library")]
[assembly: AssemblyCompany("Kirill Shlenskiy")]
[assembly: AssemblyProduct("Kirkin")]
[assembly: AssemblyCopyright("Copyright © Kirill Shlenskiy 2016")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("95dc82d1-b7c8-4f10-a4a0-747a63382eb6")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.6.4")]

// Allow unit testing internal members.
[assembly: InternalsVisibleTo("Kirkin.Experimental")]
[assembly: InternalsVisibleTo("Kirkin.Tests")]