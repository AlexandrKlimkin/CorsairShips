using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using PestelLib.ServerCommon.Extensions;

namespace PestelLib.ServerCommon.Threading
{
    /// <summary>
    /// Boilerplate code for doing something in separate thread 
    /// until object isn't disposed
    /// </summary>
    public abstract class DisposableLoop : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DisposableLoop));
        public const int ABORT_DELAY = 500;
        protected volatile bool _disposed;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task _task;
        public Action<bool> OnComplete = (b) => {};

        public DisposableLoop()
        {
            _task = Task.Run(() => Loop());
            _task.ReportOnFail().ContinueWith(b => OnComplete(!b.IsFaulted && b.Result));
        }

        public virtual void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            if (!_task.IsCompleted)
            {
                _cancellationTokenSource.CancelAfter(ABORT_DELAY);
                if (_task.Wait(ABORT_DELAY))
                    _task.Dispose();
            }
        }

        protected abstract void Update(CancellationToken cancellationToken);

        private void Loop()
        {
            while (!_disposed)
            {
                try
                {
                    Update(_cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    throw;
                }
            }
        }
    }
}
