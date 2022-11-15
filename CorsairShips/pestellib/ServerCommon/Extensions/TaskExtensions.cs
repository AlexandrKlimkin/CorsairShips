using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace PestelLib.ServerCommon.Extensions
{
    public static class TaskExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TaskExtensions));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns>false on errors</returns>
        public static Task<bool> ReportOnFail(this Task t)
        {
            return t.ContinueWith(OnTaskComplete);
        }

        public static void ResultToCallback(this Task t, Action callback)
        {
            t.ContinueWith(_ =>
            {
                var b = OnTaskComplete(_);
                if (b)
                    callback();
            });
        }

        public static void AnyResultToCallback(this Task t, Action<bool> callback)
        {
            t.ContinueWith(_ =>
            {
                var b = OnTaskComplete(_);
                callback(b);
            });
        }

        public static void ResultToCallback<T>(this Task<T> t, Action<T> callback)
        {
            t.ContinueWith(_ =>
            {
                var b = OnTaskComplete(_);
                if (b)
                    callback(t.Result);
            });
        }

        public static void AnyResultToCallback<T>(this Task<T> t, Action<bool, T> callback)
        {
            t.ContinueWith(_ =>
            {
                var b = OnTaskComplete(_);
                if (b)
                    callback(b, t.Result);
                else
                    callback(b, default(T));
            });
        }

        private static bool OnTaskComplete(Task t)
        {
            if (!t.IsFaulted)
                return true;

            var exeptions = t.Exception.Flatten().InnerExceptions;
            for (var i = 0; i < exeptions.Count; ++i)
            {
                Log.Error(exeptions[i]);
            }
            return false;
        }
    }
}
