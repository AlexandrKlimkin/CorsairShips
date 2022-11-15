FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["BoltKubernetesTest/BoltKubernetesTest.csproj", "BoltKubernetesTest/"]
COPY ["Newtonsoft.JsonDotNet/Newtonsoft.Json.csproj", "Newtonsoft.JsonDotNet/"]
RUN dotnet restore "BoltKubernetesTest/BoltKubernetesTest.csproj"
COPY . .
WORKDIR "/src/BoltKubernetesTest"
RUN dotnet build "BoltKubernetesTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BoltKubernetesTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoltKubernetesTest.dll"]