using System;
using System.Net;
using System.Net.Sockets;
using BoltTransport;
using System.Threading.Tasks;
using System.Threading;
using MasterServerProtocol;
using Autofac;
using BoltMasterServer.Connections;
using log4net;
using System.Xml;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace BoltMasterServer
{
    public class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        /*
         * Инициализация DependnenciInjection; не стал использовать наш обычный UnityDI т.к.
         * он плохо подходит для инъекции параметров через конструктор - а мне хотелось показать 
         * все зависимости явно: так проще потом тесты делать
         * Тут используется Autofac
         */
        static Program()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Settings>().AsSelf().SingleInstance();

            if (Environment.GetCommandLineArgs().Contains("-noagones"))
            {
                builder.RegisterType<GameServerLauncher>().As<IGameServerLauncher>().SingleInstance();
            }
            else
            {
                builder.RegisterType<AgonesGameServerLauncher>().As<IGameServerLauncher>().SingleInstance();
            }

            builder.RegisterType<GameServersCollection>().AsSelf().SingleInstance();

            builder.Register((componentContext) => new Watchdog(
                componentContext.Resolve<Settings>(),
                componentContext.Resolve<IGameServerLauncher>(),
                componentContext.Resolve<GameServersCollection>())).AsSelf().SingleInstance();

            builder.Register((componentContext) => new Statistics(
                componentContext.Resolve<Settings>(),
                componentContext.Resolve<GameServersCollection>()))
                .AsSelf().SingleInstance();
            //builder.RegisterType<Server<GameServerConnection>>().AsSelf();

            builder.Register((componentContext) => new GameServerConnection(
                componentContext.Resolve<GameServersCollection>(),
                componentContext.Resolve<Settings>()))
                .AsSelf();

            //https://stackoverflow.com/questions/20583339/autofac-and-func-factories
            builder.Register((componentContext) => {
                var context = componentContext.Resolve<IComponentContext>();
                var masterPort = context.Resolve<Settings>().MasterListenerPort;
                Func<GameServerConnection> createConnectionFunc = context.Resolve<GameServerConnection>;

                return new Server<GameServerConnection>(masterPort, createConnectionFunc);
                })
                .AsSelf().SingleInstance();

            builder.Register((componentContext) => new LoadBalancingConnection(
                componentContext.Resolve<Settings>(),
                componentContext.Resolve<IGameServerLauncher>(),
                componentContext.Resolve<GameServersCollection>()))
                .AsSelf();

            builder.Register((componentContext) => {
                var context = componentContext.Resolve<IComponentContext>();
                var settings = context.Resolve<Settings>();
                var ipAddress = settings.LoadBalancingIp;
                var port = settings.LoadBalancingPort;
                var connection = context.Resolve<LoadBalancingConnection>();

                return new Client<LoadBalancingConnection>(connection, ipAddress, port);
            })
            .AsSelf().SingleInstance();

            builder.RegisterType<Statistics>().AsSelf().SingleInstance();
            builder.RegisterType<Watchdog>().AsSelf().SingleInstance();

            Container = builder.Build();
        }

        private static IContainer Container { get; set; }

        public static async Task Main(string[] args)
        {
            SetupLog4net();

            /*
             * Вспомогательная функция для разбора аргументов командной строки. Надо бы куда-то вынести, на юнити клиенте такая же есть
             */
            static string GetArg(params string[] names)
            {
                var args = Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    foreach (var name in names)
                    {
                        if (args[i].Equals(name, StringComparison.InvariantCultureIgnoreCase) && args.Length > i + 1)
                        {
                            return args[i + 1];
                        }
                    }
                }

                return null;
            }

            /*
             * Определение локального IP адреса, для тестирования в локальной сети этого достаточно
             * TODO: На прод серверах нужно будет или дописать её, чтобы она находила внешний ip адрес, или задавать этот адрес в конфиге
             */
            static string GetLocalIPAddress()
            {
                var addressInEnvironmentVariable = Environment.GetEnvironmentVariable("NODE_EXTERNAL_IP");
                if (addressInEnvironmentVariable != null) {
                    return addressInEnvironmentVariable;
                }

                var address = GetArg("-address");
                if (address != null)
                {
                    return address;
                }

                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                throw new Exception("No network adapters with an IPv4 address in the system!");
            }

            using (var lifescope = Container.BeginLifetimeScope())
            {
                var settings = lifescope.Resolve<Settings>();
                var gameServerLauncher = lifescope.Resolve<IGameServerLauncher>();

                /*
                 * Сколько максимально игровых серверов может запустить данный мастер сервер
                 * TODO: добавить проверку кол-ва серверов в обработке запроса CreateGameRequest
                 */
                if (int.TryParse(GetArg("-instances"), out var instances))
                {
                    settings.MaxServers = instances;
                }

                //Путь к исполняему файлу юнити билда. Билд может быть в т.ч. под Linux собран
                settings.BuildFilePath = GetArg("-path") ?? "PhotonBoltTest.exe";

                settings.MasterServerIp = GetLocalIPAddress();
                settings.LoadBalancingIp = GetArg("-loadbalancingIp") ?? GetLocalIPAddress();

                log.Info(settings.BuildFilePath + " " + settings.MasterServerIp + " " + settings.LoadBalancingIp + " " + settings.LoadBalancingPort);

                //при завершении работы закрываем все запущенные игровые серверы - а то их никто никогда не остановит
                AppDomain.CurrentDomain.ProcessExit += new EventHandler((sender, args) => gameServerLauncher.StopAllProcesses());

                //каждый мастер клиент поддерживает постояннное соединение с лоад-балансингом
                var loadBalancingClient = lifescope.Resolve<Client<LoadBalancingConnection>>();                

                /*
                 * запрашиваем у лоадбалансинга конфигурацию, LB знает про все запущенные мастер-серверы и распределяет им 
                 * диапазоны игровых серверов таким образом, чтобы они не конфликтовали
                 * Кроме того, он выдает им свободный порт для подключения к мастер серверу игровых серверов
                 */
                var masterConfiguration = await loadBalancingClient.SendMessageAsync<MasterConfigurationResponse>(new MasterConfigurationRequest
                {
                    IPAdress = settings.LoadBalancingIp,
                    Report = loadBalancingClient.Connection.GetReport()
                });

                log.Info("Received configuration: " + masterConfiguration.MasterListenerPort + " " + masterConfiguration.FirstGameServerPort);

                //меняем свой конфиг в соответствии с тем, что пришло от мастер клиента
                settings.MasterListenerPort = masterConfiguration.MasterListenerPort;
                settings.FirstGameServerPort = masterConfiguration.FirstGameServerPort;

                /*
                 * запускаем беспонечный процесс отправки на лоадбалансинг сводного состояния:
                 * своего собственного, и всех игровых серверов которые управляются на данный 
                 * момент этим мастером
                 */
                _ = Task.Run(() => loadBalancingClient.Connection.ReportToLoadBalancing());
                               
                //создаём инстанс сервера, который будет принимать сообщения от игровых серверов
                lifescope.Resolve<Server<GameServerConnection>>();
                //статистика пока просто печатает периодически отчет в консоль
                lifescope.Resolve<Statistics>();
                //watchdog останавливает игровые сервера, которые зависли и перестали посылать своё состояние мастеру
                lifescope.Resolve<Watchdog>();

                await Task.Delay(Timeout.Infinite);
            }
        }
        private static void SetupLog4net()
        {
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("log4net.config"));
            var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(),
                       typeof(log4net.Repository.Hierarchy.Hierarchy));
            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
        }
    }
}
