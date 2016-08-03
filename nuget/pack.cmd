"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" ..\src\Build\Kirkin-NET_40\Kirkin-NET_40.csproj /p:Configuration=Release /p:Platform=AnyCPU /target:Rebuild /verbosity:minimal
"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" ..\src\Build\Kirkin-NET_45\Kirkin-NET_45.csproj /p:Configuration=Release /p:Platform=AnyCPU /target:Rebuild /verbosity:minimal
nuget pack Kirkin.nuspec -IncludeReferencedProjects -Properties Configuration=Release;Platform=AnyCPU
nuget pack Kirkin.Sources.nuspec -IncludeReferencedProjects