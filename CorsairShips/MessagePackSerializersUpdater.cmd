c:
cd c:\development\git\corsairships\corsairships
pestellib\Tools\MessagePack\mpc\mpc.exe -i "Assembly-CSharp-firstpass.csproj" -o "Assets\MessagePackSerializers_Plugins.cs" -r "GeneratedResolverPlugins"
pestellib\Tools\MessagePack\mpc\mpc.exe -i "Assembly-CSharp.csproj" -o "Assets\MessagePackSerializers_Normal.cs" -r "GeneratedResolverNormal"
echo.&pause&goto:eof