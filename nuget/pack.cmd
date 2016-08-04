"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" ..\src\Kirkin.sln /p:Configuration=Release /p:Platform="Any CPU" /target:Rebuild /verbosity:minimal || exit /b
nuget pack Kirkin.nuspec -IncludeReferencedProjects -Properties Configuration=Release || exit /b
nuget pack Kirkin.Sources.nuspec -IncludeReferencedProjects