cd ..\..\..\
.\pestellib\Tools\nuget.exe restore .\PestelProjectLib.sln
MSBuild.exe .\PestelProjectLib.sln /t:Clean,Build
pause
