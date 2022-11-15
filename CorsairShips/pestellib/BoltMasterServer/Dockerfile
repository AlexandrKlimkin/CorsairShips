FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 25000-35000/udp

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["BoltMasterServer/log4net.config", "BoltMasterServer/"]
COPY ["BoltMasterServer/BoltMasterServer.csproj", "BoltMasterServer/"]
COPY ["BoltTransport/BoltTransport.csproj", "BoltTransport/"]
COPY ["AsyncBinaryReaderWriter/AsyncBinaryReaderWriter.csproj", "AsyncBinaryReaderWriter/"]
COPY ["BoltProtocol/BoltProtocol.csproj", "BoltProtocol/"]
COPY ["Newtonsoft.JsonDotNet/Newtonsoft.Json.csproj", "Newtonsoft.JsonDotNet/"]
RUN dotnet restore "BoltMasterServer/BoltMasterServer.csproj"
COPY . .
WORKDIR "/src/BoltMasterServer"
RUN dotnet build "BoltMasterServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BoltMasterServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoltMasterServer.dll", "-path", "UnityGameBuild/build.x86_64",  "-loadbalancingIp", "192.168.88.243", "-address", "192.168.88.243"]
