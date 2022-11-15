using System;
using System.Threading;
using System.Timers;
using FriendsClient;
using FriendsClientWpf.Properties;
using PestelLib.Utils;
using ServerShared;
using UnityDI;

namespace FriendsClientWpf
{
    class FakeSharedTime : ITimeProvider
    {
        public bool IsSynced => true;
        public DateTime Now => DateTime.UtcNow;
    }

    class UpdateProviderController : IUpdateProvider
    {
        private System.Timers.Timer _timer;
        public event Action OnUpdate;
        public bool Paused { get; private set; }
        public long UpdateCount { get; private set; }
        private int concurrency;

        public UpdateProviderController()
        {
            _timer = new System.Timers.Timer(ConfigLoader.Instance.UpdatePeriod.TotalMilliseconds);
            _timer.Elapsed += tick;
            _timer.AutoReset = true;
            Paused = true;
        }

        private void tick(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                var c = Interlocked.Increment(ref concurrency);
                if (c > 1)
                    return;
                ++UpdateCount;
                OnUpdate();
            }
            finally
            {
                Interlocked.Decrement(ref concurrency);
            }
        }

        public void SetSpeed(TimeSpan period)
        {
            _timer.Interval = period.TotalMilliseconds;
        }

        public void Step()
        {
            if(_timer.Enabled) return;

            tick(null, null);
        }

        public void Pause()
        {
            _timer.Stop();
            Paused = true;
        }

        public void Resume()
        {
            _timer.Start();
            Paused = false;
        }
    }

    class InitDependencyInjection
    {
        public static void Init()
        {
            var c = ContainerHolder.Container;
            c.RegisterCustom<ITimeProvider>(() => c.Resolve<FakeSharedTime>());
            c.RegisterSingleton<FakeSharedTime>();
            c.RegisterSingleton<UpdateProviderController>();
            c.RegisterCustom<IUpdateProvider>(() => c.Resolve<UpdateProviderController>());
            if (!string.IsNullOrEmpty(ConfigLoader.Instance.FriendsServerUrl))
            {
                var serverUrl = new Uri(ConfigLoader.Instance.FriendsServerUrl);
                c.RegisterInstance<IFriendsClientTransportFactory>(new DefaultFriendsClientTransportFactory(serverUrl.Host, serverUrl.Port));
            }
            else
            {
                c.RegisterCustom<IFriendsClientTransportFactory>(() => DefaultFriendsClientTransportFactory.Instance);
            }
        }
    }
}
