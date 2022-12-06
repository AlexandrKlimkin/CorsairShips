
@echo off

pushd "%~dp0"
echo %cd%

rem rd /s /q "Assets\Plugins\LibsSymlinks"
mkdir "Assets\Plugins\LibsSymlinks"
mkdir "Assets\Plugins\LibsSymlinks\Editor"

rem BODY_START

pestellib\Tools\PackageManager\win\PackageManager.exe --UpdateDependencies
if exist "Assets\Plugins\LibsSymlinks\ConcreteSharedLogic\" (
rmdir "Assets\Plugins\LibsSymlinks\ConcreteSharedLogic" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ConcreteSharedLogic" (
del "Assets\Plugins\LibsSymlinks\ConcreteSharedLogic" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\ConcreteSharedLogic" "..\..\..\ProjectLib\ConcreteSharedLogic\Sources"
if exist "Assets\Plugins\LibsSymlinks\APKIntegrityVerifier\" (
rmdir "Assets\Plugins\LibsSymlinks\APKIntegrityVerifier" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\APKIntegrityVerifier" (
del "Assets\Plugins\LibsSymlinks\APKIntegrityVerifier" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\APKIntegrityVerifier" "..\..\..\PestelLib\APKIntegrityVerifier\Sources"
if exist "Assets\Plugins\LibsSymlinks\Editor\AssetBundleBrowser.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\AssetBundleBrowser.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\AssetBundleBrowser.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\AssetBundleBrowser.Editor" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\AssetBundles2\" (
rmdir "Assets\Plugins\LibsSymlinks\AssetBundles2" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\AssetBundles2" (
del "Assets\Plugins\LibsSymlinks\AssetBundles2" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\AssetBundlesManager\" (
rmdir "Assets\Plugins\LibsSymlinks\AssetBundlesManager" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\AssetBundlesManager" (
del "Assets\Plugins\LibsSymlinks\AssetBundlesManager" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\Editor\AssetBundlesManager.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\AssetBundlesManager.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\AssetBundlesManager.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\AssetBundlesManager.Editor" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\AsyncBinaryReaderWriter\" (
rmdir "Assets\Plugins\LibsSymlinks\AsyncBinaryReaderWriter" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\AsyncBinaryReaderWriter" (
del "Assets\Plugins\LibsSymlinks\AsyncBinaryReaderWriter" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\BadWordFilter\" (
rmdir "Assets\Plugins\LibsSymlinks\BadWordFilter" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\BadWordFilter" (
del "Assets\Plugins\LibsSymlinks\BadWordFilter" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\BestHTTP\" (
rmdir "Assets\Plugins\LibsSymlinks\BestHTTP" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\BestHTTP" (
del "Assets\Plugins\LibsSymlinks\BestHTTP" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\BestHTTP" "..\..\..\PestelLib\BestHTTP\Sources"
if exist "Assets\Plugins\LibsSymlinks\BoltMasterServerClient\" (
rmdir "Assets\Plugins\LibsSymlinks\BoltMasterServerClient" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\BoltMasterServerClient" (
del "Assets\Plugins\LibsSymlinks\BoltMasterServerClient" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\BoltProtocol\" (
rmdir "Assets\Plugins\LibsSymlinks\BoltProtocol" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\BoltProtocol" (
del "Assets\Plugins\LibsSymlinks\BoltProtocol" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\BoltTransport\" (
rmdir "Assets\Plugins\LibsSymlinks\BoltTransport" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\BoltTransport" (
del "Assets\Plugins\LibsSymlinks\BoltTransport" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\Editor\BuildUtils.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\BuildUtils.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\BuildUtils.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\BuildUtils.Editor" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\CameraTools\" (
rmdir "Assets\Plugins\LibsSymlinks\CameraTools" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\CameraTools" (
del "Assets\Plugins\LibsSymlinks\CameraTools" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\CameraTools" "..\..\..\PestelLib\CameraTools\Sources"
if exist "Assets\Plugins\LibsSymlinks\ChatClient\" (
rmdir "Assets\Plugins\LibsSymlinks\ChatClient" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ChatClient" (
del "Assets\Plugins\LibsSymlinks\ChatClient" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\ChatCommon\" (
rmdir "Assets\Plugins\LibsSymlinks\ChatCommon" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ChatCommon" (
del "Assets\Plugins\LibsSymlinks\ChatCommon" /f /q
)

if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\Chests\" (
rmdir "ProjectLib\ConcreteSharedLogic\Sources\Modules\Chests" /s /q
)
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\Chests" (
del "ProjectLib\ConcreteSharedLogic\Sources\Modules\Chests" / f /q
)

if exist "Assets\Plugins\LibsSymlinks\ClansClient\" (
rmdir "Assets\Plugins\LibsSymlinks\ClansClient" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ClansClient" (
del "Assets\Plugins\LibsSymlinks\ClansClient" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\Editor\CleanEmptyDir.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\CleanEmptyDir.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\CleanEmptyDir.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\CleanEmptyDir.Editor" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\ClientConfig\" (
rmdir "Assets\Plugins\LibsSymlinks\ClientConfig" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ClientConfig" (
del "Assets\Plugins\LibsSymlinks\ClientConfig" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\ClientConfig" "..\..\..\PestelLib\ClientConfig\Sources"
if exist "Assets\Plugins\LibsSymlinks\Editor\ClientConfig.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\ClientConfig.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\ClientConfig.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\ClientConfig.Editor" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\Editor\ClientConfig.Editor" "..\..\..\..\PestelLib\ClientConfig.Editor\Sources"

if exist "Assets\Plugins\LibsSymlinks\Editor\ConsolePro.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\ConsolePro.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\ConsolePro.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\ConsolePro.Editor" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\CrossplatfomInput\" (
rmdir "Assets\Plugins\LibsSymlinks\CrossplatfomInput" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\CrossplatfomInput" (
del "Assets\Plugins\LibsSymlinks\CrossplatfomInput" /f /q
)

if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\DailyRewards\" (
rmdir "ProjectLib\ConcreteSharedLogic\Sources\Modules\DailyRewards" /s /q
)
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\DailyRewards" (
del "ProjectLib\ConcreteSharedLogic\Sources\Modules\DailyRewards" / f /q
)

if exist "Assets\Plugins\LibsSymlinks\DeadlyFastFSM\" (
rmdir "Assets\Plugins\LibsSymlinks\DeadlyFastFSM" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\DeadlyFastFSM" (
del "Assets\Plugins\LibsSymlinks\DeadlyFastFSM" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\Editor\DeadlyFastFSM.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\DeadlyFastFSM.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\DeadlyFastFSM.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\DeadlyFastFSM.Editor" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\DebugMenu\" (
rmdir "Assets\Plugins\LibsSymlinks\DebugMenu" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\DebugMenu" (
del "Assets\Plugins\LibsSymlinks\DebugMenu" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\DebugMenu" "..\..\..\PestelLib\DebugMenu\Sources"
if exist "Assets\Plugins\LibsSymlinks\DeepWatersPostProcessing\" (
rmdir "Assets\Plugins\LibsSymlinks\DeepWatersPostProcessing" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\DeepWatersPostProcessing" (
del "Assets\Plugins\LibsSymlinks\DeepWatersPostProcessing" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\DependencyInjection\" (
rmdir "Assets\Plugins\LibsSymlinks\DependencyInjection" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\DependencyInjection" (
del "Assets\Plugins\LibsSymlinks\DependencyInjection" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\DependencyInjection" "..\..\..\PestelLib\DependencyInjection\Sources"
if exist "Assets\Plugins\LibsSymlinks\DependencyInjectionCore\" (
rmdir "Assets\Plugins\LibsSymlinks\DependencyInjectionCore" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\DependencyInjectionCore" (
del "Assets\Plugins\LibsSymlinks\DependencyInjectionCore" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\DependencyInjectionCore" "..\..\..\PestelLib\DependencyInjectionCore\Sources"
if exist "Assets\Plugins\LibsSymlinks\EditorSharedPrefs\" (
rmdir "Assets\Plugins\LibsSymlinks\EditorSharedPrefs" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\EditorSharedPrefs" (
del "Assets\Plugins\LibsSymlinks\EditorSharedPrefs" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\EloRating\" (
rmdir "Assets\Plugins\LibsSymlinks\EloRating" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\EloRating" (
del "Assets\Plugins\LibsSymlinks\EloRating" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\FriendsClient\" (
rmdir "Assets\Plugins\LibsSymlinks\FriendsClient" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\FriendsClient" (
del "Assets\Plugins\LibsSymlinks\FriendsClient" /f /q
)

if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\GlobalConflictModule\" (
rmdir "ProjectLib\ConcreteSharedLogic\Sources\Modules\GlobalConflictModule" /s /q
)
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\GlobalConflictModule" (
del "ProjectLib\ConcreteSharedLogic\Sources\Modules\GlobalConflictModule" / f /q
)

if exist "Assets\Plugins\LibsSymlinks\GoogleSpreadsheet\" (
rmdir "Assets\Plugins\LibsSymlinks\GoogleSpreadsheet" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\GoogleSpreadsheet" (
del "Assets\Plugins\LibsSymlinks\GoogleSpreadsheet" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\GoogleSpreadsheet" "..\..\..\PestelLib\GoogleSpreadsheet\Sources"
if exist "Assets\Plugins\LibsSymlinks\Editor\GoogleSpreadsheet.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\GoogleSpreadsheet.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\GoogleSpreadsheet.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\GoogleSpreadsheet.Editor" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\Editor\GoogleSpreadsheet.Editor" "..\..\..\..\PestelLib\GoogleSpreadsheet.Editor\Sources"

if exist "Assets\Plugins\LibsSymlinks\IapValidatorClient\" (
rmdir "Assets\Plugins\LibsSymlinks\IapValidatorClient" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\IapValidatorClient" (
del "Assets\Plugins\LibsSymlinks\IapValidatorClient" /f /q
)

if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\LeaguesModule\" (
rmdir "ProjectLib\ConcreteSharedLogic\Sources\Modules\LeaguesModule" /s /q
)
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\LeaguesModule" (
del "ProjectLib\ConcreteSharedLogic\Sources\Modules\LeaguesModule" / f /q
)

if exist "Assets\Plugins\LibsSymlinks\Lidgren.Network\" (
rmdir "Assets\Plugins\LibsSymlinks\Lidgren.Network" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Lidgren.Network" (
del "Assets\Plugins\LibsSymlinks\Lidgren.Network" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\Localization\" (
rmdir "Assets\Plugins\LibsSymlinks\Localization" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Localization" (
del "Assets\Plugins\LibsSymlinks\Localization" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\Localization" "..\..\..\PestelLib\Localization\Sources"
if exist "Assets\Plugins\LibsSymlinks\Log\" (
rmdir "Assets\Plugins\LibsSymlinks\Log" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Log" (
del "Assets\Plugins\LibsSymlinks\Log" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\log4net\" (
rmdir "Assets\Plugins\LibsSymlinks\log4net" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\log4net" (
del "Assets\Plugins\LibsSymlinks\log4net" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\log4net" "..\..\..\PestelLib\log4net\Sources"
if exist "Assets\Plugins\LibsSymlinks\MatchmakerShared\" (
rmdir "Assets\Plugins\LibsSymlinks\MatchmakerShared" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\MatchmakerShared" (
del "Assets\Plugins\LibsSymlinks\MatchmakerShared" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\MecanimExtensions\" (
rmdir "Assets\Plugins\LibsSymlinks\MecanimExtensions" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\MecanimExtensions" (
del "Assets\Plugins\LibsSymlinks\MecanimExtensions" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\MecanimTransitionStorage\" (
rmdir "Assets\Plugins\LibsSymlinks\MecanimTransitionStorage" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\MecanimTransitionStorage" (
del "Assets\Plugins\LibsSymlinks\MecanimTransitionStorage" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\MessageClient\" (
rmdir "Assets\Plugins\LibsSymlinks\MessageClient" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\MessageClient" (
del "Assets\Plugins\LibsSymlinks\MessageClient" /f /q
)

if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\MessageInboxModule\" (
rmdir "ProjectLib\ConcreteSharedLogic\Sources\Modules\MessageInboxModule" /s /q
)
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\MessageInboxModule" (
del "ProjectLib\ConcreteSharedLogic\Sources\Modules\MessageInboxModule" / f /q
)

if exist "Assets\Plugins\LibsSymlinks\MessagePack\" (
rmdir "Assets\Plugins\LibsSymlinks\MessagePack" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\MessagePack" (
del "Assets\Plugins\LibsSymlinks\MessagePack" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\MessagePack" "..\..\..\PestelLib\MessagePack\Sources"
if exist "Assets\Plugins\LibsSymlinks\NetworkUtils\" (
rmdir "Assets\Plugins\LibsSymlinks\NetworkUtils" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\NetworkUtils" (
del "Assets\Plugins\LibsSymlinks\NetworkUtils" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\NetworkUtils" "..\..\..\PestelLib\NetworkUtils\Sources"
if exist "Assets\Plugins\LibsSymlinks\Newtonsoft.JsonDotNet\" (
rmdir "Assets\Plugins\LibsSymlinks\Newtonsoft.JsonDotNet" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Newtonsoft.JsonDotNet" (
del "Assets\Plugins\LibsSymlinks\Newtonsoft.JsonDotNet" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\Newtonsoft.JsonDotNet" "..\..\..\PestelLib\Newtonsoft.JsonDotNet\Sources"
if exist "Assets\Plugins\LibsSymlinks\ObjectsPool\" (
rmdir "Assets\Plugins\LibsSymlinks\ObjectsPool" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ObjectsPool" (
del "Assets\Plugins\LibsSymlinks\ObjectsPool" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\Editor\PackageManager.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\PackageManager.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\PackageManager.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\PackageManager.Editor" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\Editor\PackageManager.Editor" "..\..\..\..\PestelLib\PackageManager.Editor\Sources"

if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\PestelLeaderboard\" (
rmdir "ProjectLib\ConcreteSharedLogic\Sources\Modules\PestelLeaderboard" /s /q
)
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\PestelLeaderboard" (
del "ProjectLib\ConcreteSharedLogic\Sources\Modules\PestelLeaderboard" / f /q
)

if exist "Assets\Plugins\LibsSymlinks\PlayerProfile\" (
rmdir "Assets\Plugins\LibsSymlinks\PlayerProfile" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\PlayerProfile" (
del "Assets\Plugins\LibsSymlinks\PlayerProfile" /f /q
)

if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\PremiumShop\" (
rmdir "ProjectLib\ConcreteSharedLogic\Sources\Modules\PremiumShop" /s /q
)
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\PremiumShop" (
del "ProjectLib\ConcreteSharedLogic\Sources\Modules\PremiumShop" / f /q
)

if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\PromoModule\" (
rmdir "ProjectLib\ConcreteSharedLogic\Sources\Modules\PromoModule" /s /q
)
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\PromoModule" (
del "ProjectLib\ConcreteSharedLogic\Sources\Modules\PromoModule" / f /q
)

if exist "Assets\Plugins\LibsSymlinks\ProxyClient\" (
rmdir "Assets\Plugins\LibsSymlinks\ProxyClient" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ProxyClient" (
del "Assets\Plugins\LibsSymlinks\ProxyClient" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\QualitySwitcher\" (
rmdir "Assets\Plugins\LibsSymlinks\QualitySwitcher" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\QualitySwitcher" (
del "Assets\Plugins\LibsSymlinks\QualitySwitcher" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\Editor\QualitySwitcher.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\QualitySwitcher.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\QualitySwitcher.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\QualitySwitcher.Editor" /f /q
)

if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\Quests\" (
rmdir "ProjectLib\ConcreteSharedLogic\Sources\Modules\Quests" /s /q
)
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\Quests" (
del "ProjectLib\ConcreteSharedLogic\Sources\Modules\Quests" / f /q
)

if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\RandomModule\" (
rmdir "ProjectLib\ConcreteSharedLogic\Sources\Modules\RandomModule" /s /q
)
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\RandomModule" (
del "ProjectLib\ConcreteSharedLogic\Sources\Modules\RandomModule" / f /q
)
mklink /d "ProjectLib\ConcreteSharedLogic\Sources\Modules\RandomModule" "..\..\..\..\PestelLib\RandomModule\SourcesSL"

if exist "Assets\Plugins\LibsSymlinks\Replay\" (
rmdir "Assets\Plugins\LibsSymlinks\Replay" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Replay" (
del "Assets\Plugins\LibsSymlinks\Replay" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\ReportPlayersClient\" (
rmdir "Assets\Plugins\LibsSymlinks\ReportPlayersClient" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ReportPlayersClient" (
del "Assets\Plugins\LibsSymlinks\ReportPlayersClient" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\ReportPlayersProtocol\" (
rmdir "Assets\Plugins\LibsSymlinks\ReportPlayersProtocol" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ReportPlayersProtocol" (
del "Assets\Plugins\LibsSymlinks\ReportPlayersProtocol" /f /q
)

if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\Roulette\" (
rmdir "ProjectLib\ConcreteSharedLogic\Sources\Modules\Roulette" /s /q
)
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\Roulette" (
del "ProjectLib\ConcreteSharedLogic\Sources\Modules\Roulette" / f /q
)

if exist "Assets\Plugins\LibsSymlinks\SaveSystem\" (
rmdir "Assets\Plugins\LibsSymlinks\SaveSystem" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\SaveSystem" (
del "Assets\Plugins\LibsSymlinks\SaveSystem" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\SaveSystem" "..\..\..\PestelLib\SaveSystem\Sources"
if exist "Assets\Plugins\LibsSymlinks\SendFeedback\" (
rmdir "Assets\Plugins\LibsSymlinks\SendFeedback" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\SendFeedback" (
del "Assets\Plugins\LibsSymlinks\SendFeedback" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\ServerClientUtils\" (
rmdir "Assets\Plugins\LibsSymlinks\ServerClientUtils" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ServerClientUtils" (
del "Assets\Plugins\LibsSymlinks\ServerClientUtils" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\ServerClientUtils" "..\..\..\PestelLib\ServerClientUtils\Sources"
if exist "Assets\Plugins\LibsSymlinks\ServerLogClient\" (
rmdir "Assets\Plugins\LibsSymlinks\ServerLogClient" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ServerLogClient" (
del "Assets\Plugins\LibsSymlinks\ServerLogClient" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\ServerLogClient" "..\..\..\PestelLib\ServerLogClient\Sources"
if exist "Assets\Plugins\LibsSymlinks\ServerLogProtocol\" (
rmdir "Assets\Plugins\LibsSymlinks\ServerLogProtocol" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ServerLogProtocol" (
del "Assets\Plugins\LibsSymlinks\ServerLogProtocol" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\ServerLogProtocol" "..\..\..\PestelLib\ServerLogProtocol\Sources"
if exist "Assets\Plugins\LibsSymlinks\ServerProtocol\" (
rmdir "Assets\Plugins\LibsSymlinks\ServerProtocol" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ServerProtocol" (
del "Assets\Plugins\LibsSymlinks\ServerProtocol" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\ServerProtocol" "..\..\..\PestelLib\ServerProtocol\Sources"
if exist "Assets\Plugins\LibsSymlinks\ServerShared\" (
rmdir "Assets\Plugins\LibsSymlinks\ServerShared" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ServerShared" (
del "Assets\Plugins\LibsSymlinks\ServerShared" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\ServerShared" "..\..\..\PestelLib\ServerShared\Sources"
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\SettingsModule\" (
rmdir "ProjectLib\ConcreteSharedLogic\Sources\Modules\SettingsModule" /s /q
)
if exist "ProjectLib\ConcreteSharedLogic\Sources\Modules\SettingsModule" (
del "ProjectLib\ConcreteSharedLogic\Sources\Modules\SettingsModule" / f /q
)

if exist "Assets\Plugins\LibsSymlinks\SharedConfig\" (
rmdir "Assets\Plugins\LibsSymlinks\SharedConfig" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\SharedConfig" (
del "Assets\Plugins\LibsSymlinks\SharedConfig" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\SharedConfig" "..\..\..\PestelLib\SharedConfig\Sources"
if exist "Assets\Plugins\LibsSymlinks\SharedLogicBase\" (
rmdir "Assets\Plugins\LibsSymlinks\SharedLogicBase" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\SharedLogicBase" (
del "Assets\Plugins\LibsSymlinks\SharedLogicBase" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\SharedLogicBase" "..\..\..\PestelLib\SharedLogicBase\Sources"
if exist "Assets\Plugins\LibsSymlinks\Editor\SharedLogicBase.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\SharedLogicBase.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\SharedLogicBase.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\SharedLogicBase.Editor" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\Editor\SharedLogicBase.Editor" "..\..\..\..\PestelLib\SharedLogicBase.Editor\Sources"

if exist "Assets\Plugins\LibsSymlinks\SharedLogicClient\" (
rmdir "Assets\Plugins\LibsSymlinks\SharedLogicClient" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\SharedLogicClient" (
del "Assets\Plugins\LibsSymlinks\SharedLogicClient" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\SharedLogicClient" "..\..\..\PestelLib\SharedLogicClient\Sources"
if exist "Assets\Plugins\LibsSymlinks\SharedLogicTemplate\" (
rmdir "Assets\Plugins\LibsSymlinks\SharedLogicTemplate" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\SharedLogicTemplate" (
del "Assets\Plugins\LibsSymlinks\SharedLogicTemplate" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\ShortPlayerIdClient\" (
rmdir "Assets\Plugins\LibsSymlinks\ShortPlayerIdClient" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ShortPlayerIdClient" (
del "Assets\Plugins\LibsSymlinks\ShortPlayerIdClient" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\ShortPlayerIdProtocol\" (
rmdir "Assets\Plugins\LibsSymlinks\ShortPlayerIdProtocol" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\ShortPlayerIdProtocol" (
del "Assets\Plugins\LibsSymlinks\ShortPlayerIdProtocol" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\SignalsRoboCars\" (
rmdir "Assets\Plugins\LibsSymlinks\SignalsRoboCars" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\SignalsRoboCars" (
del "Assets\Plugins\LibsSymlinks\SignalsRoboCars" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\SimpleLOD\" (
rmdir "Assets\Plugins\LibsSymlinks\SimpleLOD" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\SimpleLOD" (
del "Assets\Plugins\LibsSymlinks\SimpleLOD" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\Editor\SimpleLOD.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\SimpleLOD.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\SimpleLOD.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\SimpleLOD.Editor" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\SQLite\" (
rmdir "Assets\Plugins\LibsSymlinks\SQLite" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\SQLite" (
del "Assets\Plugins\LibsSymlinks\SQLite" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\SQLite" "..\..\..\PestelLib\SQLite\Sources"
if exist "Assets\Plugins\LibsSymlinks\TaskQueueLib\" (
rmdir "Assets\Plugins\LibsSymlinks\TaskQueueLib" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\TaskQueueLib" (
del "Assets\Plugins\LibsSymlinks\TaskQueueLib" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\TaskQueueLib" "..\..\..\PestelLib\TaskQueueLib\Sources"
if exist "Assets\Plugins\LibsSymlinks\TasksCommon\" (
rmdir "Assets\Plugins\LibsSymlinks\TasksCommon" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\TasksCommon" (
del "Assets\Plugins\LibsSymlinks\TasksCommon" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\TasksCommon" "..\..\..\PestelLib\TasksCommon\Sources"
if exist "Assets\Plugins\LibsSymlinks\UI\" (
rmdir "Assets\Plugins\LibsSymlinks\UI" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\UI" (
del "Assets\Plugins\LibsSymlinks\UI" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\UI" "..\..\..\PestelLib\UI\Sources"
if exist "Assets\Plugins\LibsSymlinks\UI3dLabels\" (
rmdir "Assets\Plugins\LibsSymlinks\UI3dLabels" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\UI3dLabels" (
del "Assets\Plugins\LibsSymlinks\UI3dLabels" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\UniGif\" (
rmdir "Assets\Plugins\LibsSymlinks\UniGif" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\UniGif" (
del "Assets\Plugins\LibsSymlinks\UniGif" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\Editor\UnityIAPUtils.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\UnityIAPUtils.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\UnityIAPUtils.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\UnityIAPUtils.Editor" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\UniversalSerializer\" (
rmdir "Assets\Plugins\LibsSymlinks\UniversalSerializer" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\UniversalSerializer" (
del "Assets\Plugins\LibsSymlinks\UniversalSerializer" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\UniversalSerializer" "..\..\..\PestelLib\UniversalSerializer\Sources"
if exist "Assets\Plugins\LibsSymlinks\UniversalSerializer.Unity\" (
rmdir "Assets\Plugins\LibsSymlinks\UniversalSerializer.Unity" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\UniversalSerializer.Unity" (
del "Assets\Plugins\LibsSymlinks\UniversalSerializer.Unity" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\UpdateSharedLogicVersion\" (
rmdir "Assets\Plugins\LibsSymlinks\UpdateSharedLogicVersion" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\UpdateSharedLogicVersion" (
del "Assets\Plugins\LibsSymlinks\UpdateSharedLogicVersion" /f /q
)

if exist "Assets\Plugins\LibsSymlinks\UserProfileViewer\" (
rmdir "Assets\Plugins\LibsSymlinks\UserProfileViewer" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\UserProfileViewer" (
del "Assets\Plugins\LibsSymlinks\UserProfileViewer" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\UserProfileViewer" "..\..\..\PestelLib\UserProfileViewer\Sources"
if exist "Assets\Plugins\LibsSymlinks\Editor\UserProfileViewer.Editor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\UserProfileViewer.Editor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\UserProfileViewer.Editor" (
del "Assets\Plugins\LibsSymlinks\Editor\UserProfileViewer.Editor" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\Editor\UserProfileViewer.Editor" "..\..\..\..\PestelLib\UserProfileViewer.Editor\Sources"

if exist "Assets\Plugins\LibsSymlinks\Utils\" (
rmdir "Assets\Plugins\LibsSymlinks\Utils" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Utils" (
del "Assets\Plugins\LibsSymlinks\Utils" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\Utils" "..\..\..\PestelLib\Utils\Sources"
if exist "Assets\Plugins\LibsSymlinks\Editor\UtilsEditor\" (
rmdir "Assets\Plugins\LibsSymlinks\Editor\UtilsEditor" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\Editor\UtilsEditor" (
del "Assets\Plugins\LibsSymlinks\Editor\UtilsEditor" /f /q
)
mklink /d "Assets\Plugins\LibsSymlinks\Editor\UtilsEditor" "..\..\..\..\PestelLib\UtilsEditor\Sources"

if exist "Assets\Plugins\LibsSymlinks\VoiceChat\" (
rmdir "Assets\Plugins\LibsSymlinks\VoiceChat" /s /q
)
if exist "Assets\Plugins\LibsSymlinks\VoiceChat" (
del "Assets\Plugins\LibsSymlinks\VoiceChat" /f /q
)


rem BODY_END

python PestelLib\Tools\BuildTools\pestellibgen.py --project-root=%~dp0 --sln-name=PestelProjectLib.sln

rem echo.&pause&goto:eof

popd