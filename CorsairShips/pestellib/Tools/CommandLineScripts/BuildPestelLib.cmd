cd ..\..\..\
.\pestellib\Tools\nuget.exe locals all -clear
cd pestellib
.\Tools\nuget.exe restore PestelLib.sln
cd ..
MSBuild.exe .\pestellib\PestelLib.sln /t:Clean,Build
pause