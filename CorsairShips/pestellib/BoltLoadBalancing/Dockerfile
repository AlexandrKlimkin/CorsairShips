FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 9998 9999

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["BoltLoadBalancing/log4net.config", "BoltLoadBalancing/"]
COPY ["BoltLoadBalancing/BoltLoadBalancing.csproj", "BoltLoadBalancing/"]
COPY ["BoltTransport/BoltTransport.csproj", "BoltTransport/"]
COPY ["AsyncBinaryReaderWriter/AsyncBinaryReaderWriter.csproj", "AsyncBinaryReaderWriter/"]
COPY ["BoltProtocol/BoltProtocol.csproj", "BoltProtocol/"]
COPY ["Newtonsoft.JsonDotNet/Newtonsoft.Json.csproj", "Newtonsoft.JsonDotNet/"]
RUN dotnet restore "BoltLoadBalancing/BoltLoadBalancing.csproj"
COPY . .
WORKDIR "/src/BoltLoadBalancing"
RUN dotnet build "BoltLoadBalancing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BoltLoadBalancing.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoltLoadBalancing.dll"]