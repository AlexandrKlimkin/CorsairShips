using BoltTransport;
using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using BoltLoadBalancing.Logic;
using BoltLoadBalancing.MasterServer;
using System.Xml;
using System.IO;
using System.Reflection;
using log4net;

namespace BoltLoadBalancing
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private static IContainer Container { get; }
        
        static Program()
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterType<MasterServersCollection>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<Matchmaking>().As<IMatchmaking>();

            builder.RegisterType<CreateOrJoinGameHelper>().As<ICreateOrJoinGameHelper>();

            builder.Register((context) => new SemaphoreSlim(1, 1))
                .AsSelf()
                .SingleInstance();
            
            builder.Register((componentContext) => new MasterServerConnection(
                    componentContext.Resolve<MasterServersCollection>(),
                    componentContext.Resolve<SemaphoreSlim>()))
                .AsSelf();

            builder.Register((context) => new PlayerConnection(
                context.Resolve<MasterServersCollection>(),
                context.Resolve<IMatchmaking>(),
                context.Resolve<ICreateOrJoinGameHelper>()
            )).AsSelf();
            
            Container = builder.Build();
        }

        private static void SetupLog4net()
        {
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("log4net.config"));
            var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(),
                       typeof(log4net.Repository.Hierarchy.Hierarchy));
            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
        }

        static async Task Main(string[] args)
        {
            SetupLog4net();

            /*
             * Этот семафор сейчас - один единственный на весь проект, пока он заблокирован - 
             * мастер серверы не могут запросить себе конфигурацию.
             * Но это не мешает принимать статусы от запущенных ранее мастер серверов
             */
            var semaphore = Container.Resolve<SemaphoreSlim>();
            
            _ = new Server<MasterServerConnection>(9999, Container.Resolve<MasterServerConnection>);
            log.Info("Waiting existing master servers to reconnect");
            
            /*
             * нужно время, что бы все уже запущенные мастер-серверы подключились к лоад балансингу,
             * иначе мы можем раздавать при инициализации мастеров порты, которые уже по факту кем-то заняты
             */
            try
            {
                await semaphore.WaitAsync();
                await Task.Delay(10000);
            } 
            finally
            {
                semaphore.Release();
            }

            _ = new Server<PlayerConnection>(9998, Container.Resolve<PlayerConnection>);
            log.Info("Waiting incoming connections from master servers and players");
            await Task.Delay(Timeout.Infinite);
        }
    }
}
