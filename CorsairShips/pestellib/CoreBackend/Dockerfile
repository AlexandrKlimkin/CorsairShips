FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 5150

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["pestellib/Tools/UnityEngineDlls/Editor/Data/Managed/UnityEngine.dll", "/Editor/Data/Managed/"]
COPY ["pestellib/Tools/UnityEngineDlls/Editor/Data/UnityExtensions/Unity/GUISystem/UnityEngine.UI.dll", "/Editor/Data/UnityExtensions/Unity/GUISystem/"]
ENV UNITY_HOME /

COPY ["pestellib/CoreBackend/CoreBackend.csproj", "pestellib/CoreBackend/"]
COPY ["pestellib/ServerCommon/ServerCommon.csproj", "pestellib/ServerCommon/"]
COPY ["pestellib/log4net/log4net.csproj", "pestellib/log4net/"]
COPY ["pestellib/DependencyInjectionCore/DependencyInjectionCore.csproj", "pestellib/DependencyInjectionCore/"]
COPY ["pestellib/ServerShared/ServerShared.csproj", "pestellib/ServerShared/"]
COPY ["pestellib/ServerProtocol/ServerProtocol.csproj", "pestellib/ServerProtocol/"]
COPY ["pestellib/MessagePack/MessagePack.csproj", "pestellib/MessagePack/"]
COPY ["pestellib/BackendCommon/BackendCommon.csproj", "pestellib/BackendCommon/"]
COPY ["pestellib/SharedLogicBase/SharedLogicBase.csproj", "pestellib/SharedLogicBase/"]
COPY ["pestellib/GoogleSpreadsheet/GoogleSpreadsheet.csproj", "pestellib/GoogleSpreadsheet/"]
COPY ["pestellib/UniversalSerializer/UniversalSerializer.csproj", "pestellib/UniversalSerializer/"]
COPY ["pestellib/ShortPlayerId/ShortPlayerId.csproj", "pestellib/ShortPlayerId/"]
COPY ["pestellib/ServerExtension/ServerExtension.csproj", "pestellib/ServerExtension/"]
COPY ["pestellib/ShortPlayerIdProtocol/ShortPlayerIdProtocol.csproj", "pestellib/ShortPlayerIdProtocol/"]
COPY ["pestellib/Utils/Utils.csproj", "pestellib/Utils/"]
COPY ["pestellib/DependencyInjection/DependencyInjection.csproj", "pestellib/DependencyInjection/"]
COPY ["pestellib/CameraTools/CameraTools.csproj", "pestellib/CameraTools/"]
COPY ["pestellib/DOTween/DOTween.csproj", "pestellib/DOTween/"]
COPY ["pestellib/ReportPlayers/ReportPlayers.csproj", "pestellib/ReportPlayers/"]
COPY ["pestellib/ReportPlayersProtocol/ReportPlayersProtocol.csproj", "pestellib/ReportPlayersProtocol/"]
COPY ["pestellib/ChatServerLib/ChatServer.csproj", "pestellib/ChatServerLib/"]
COPY ["pestellib/ChatCommon/ChatCommon.csproj", "pestellib/ChatCommon/"]
COPY ["pestellib/Lidgren.Network/Lidgren.Network.csproj", "pestellib/Lidgren.Network/"]
COPY ["pestellib/ServiceCommon/ServiceCommon.csproj", "pestellib/ServiceCommon/"]
COPY ["pestellib/NicknameGenerator/NicknameGenerator.csproj", "pestellib/NicknameGenerator/"]
COPY ["pestellib/SharedConfig/SharedConfig.csproj", "pestellib/SharedConfig/"]
COPY ["pestellib/Log/Log.csproj", "pestellib/Log/"]
RUN dotnet restore "pestellib/CoreBackend/CoreBackend.csproj"
COPY . .
WORKDIR "/src/pestellib/CoreBackend"
RUN dotnet build "CoreBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoreBackend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY pestellib/CoreBackend/App_Data/*.* ./App_Data/
COPY pestellib/Tools/UnityEngineDlls/Editor/Data/Managed/UnityEngine.dll .
COPY pestellib/Tools/UnityEngineDlls/Editor/Data/UnityExtensions/Unity/GUISystem/UnityEngine.UI.dll .
COPY --from=publish /app/publish .
COPY pestellib/CoreBackend/App_Data/appsettings.json ./App_Data/
ENTRYPOINT ["dotnet", "CoreBackend.dll"]