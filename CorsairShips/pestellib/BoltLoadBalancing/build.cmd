dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true
xcopy bin\Release\netcoreapp3.0\win-x64\publish\BoltLoadBalancing.exe ..\..\BuildsMono\BoltLoadBalancing.exe /Y