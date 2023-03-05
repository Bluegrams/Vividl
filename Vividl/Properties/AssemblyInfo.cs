using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;
using Bluegrams.Application.Attributes;

[assembly: AssemblyTitle("Vividl - Video Downloader")]
[assembly: AssemblyDescription("Free Video Downloader for Windows")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Bluegrams")]
[assembly: AssemblyProduct("Vividl")]
[assembly: AssemblyCopyright("Copyright © 2020-2023 Bluegrams")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ProductWebsite("https://vividl.sourceforge.io")]
[assembly: ProductLicense("LICENSE.txt", "BSD-3-clause License")]
[assembly: CompanyWebsite("http://bluegrams.com", "http://bluegrams.com")]

#if PORTABLE
[assembly: AppPortable(true)]
#endif

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

//In order to begin building localizable applications, set
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

[assembly: NeutralResourcesLanguage("en")]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]

[assembly: AssemblyVersion("0.7.0.0")]
[assembly: AssemblyFileVersion("0.7.0.0")]
