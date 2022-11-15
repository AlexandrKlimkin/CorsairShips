cd ..\..\..\
.\pestellib\Tools\nuget.exe restore .\PestelProjectLib.sln
MSBuild.exe .\PestelProjectLib.sln /t:Clean,Build
cd .\ProjectLib\ConcreteServer\bin\Debug\netcoreapp2.0\
dotnet.exe ConcreteServer.dll

