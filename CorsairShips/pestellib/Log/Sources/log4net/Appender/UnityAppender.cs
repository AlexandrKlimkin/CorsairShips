using log4net.Appender;
using log4net.Core;
using UnityEngine;

namespace PestelLib.Log.log4net.Appender
{
    /// <summary>
    /// Unity appender for log4net
    /// </summary>
    public class UnityAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            string message = RenderLoggingEvent(loggingEvent);

            if (Level.Compare(loggingEvent.Level, Level.Error) >= 0)
            {
                // everything above or equal to error is an error
                Debug.LogError(message);
            }
            else if (Level.Compare(loggingEvent.Level, Level.Warn) >= 0)
            {
                // everything that is a warning up to error is logged as warning
                Debug.LogWarning(message);
            }
            else
            {
                // everything else we'll just log normally
                Debug.Log(message);
            }
        }
    }
}
