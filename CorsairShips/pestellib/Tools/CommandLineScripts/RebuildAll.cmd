cd ..\..\..\
.\pestellib\Tools\nuget.exe locals all -clear
cd pestellib
.\Tools\nuget.exe restore
cd ..
.\pestellib\Tools\nuget.exe restore .\PestelProjectLib.sln
MSBuild.exe .\pestellib\PestelLib.sln /t:Clean,Build
pause
MSBuild.exe .\PestelProjectLib.sln /t:Clean,Build
pause
