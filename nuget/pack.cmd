"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" ..\src\Kirkin.sln /p:Configuration=Release /p:Platform="Any CPU" /target:Rebuild /verbosity:minimal
nuget pack Kirkin.nuspec -IncludeReferencedProjects -Properties Configuration=Release;Platform=AnyCPU
nuget pack Kirkin.Sources.nuspec -IncludeReferencedProjects