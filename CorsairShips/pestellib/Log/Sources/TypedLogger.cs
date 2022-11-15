using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PestelLib.Log
{
#if UNITY_2017
    public class TypedLogger<T>
    {
        private readonly string _tag;
        private readonly ILogger _logger;

        public TypedLogger()
        {
            _tag = typeof(T).ToString();
            _logger = Debug.unityLogger;
        }

        public TypedLogger(string tag)
        {
            _tag = tag;
            _logger = Debug.unityLogger;
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            _logger.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, Object context)
        {
            _logger.LogException(exception, context);
        }

        public bool IsLogTypeAllowed(LogType logType)
        {
            return _logger.IsLogTypeAllowed(logType);
        }

        public void Log(LogType logType, object message)
        {
            _logger.Log(logType, _tag, message);
        }

        public void Log(LogType logType, object message, Object context)
        {
            _logger.Log(logType, _tag, message, context);
        }

        public void Log(object message)
        {
            _logger.Log(_tag, message);
        }
        
        public void LogWarning(object message)
        {
            _logger.Log(LogType.Warning, _tag, message);
        }

        public void LogError(object message)
        {
            _logger.Log(LogType.Error, _tag, message);
        }
        
        public void LogFormat(LogType logType, string format, params object[] args)
        {
            _logger.LogFormat(logType, _tag, format, args);
        }

        public void LogException(Exception exception)
        {
            _logger.LogException(exception);
        }
    }
#else
    public class TypedLogger<T>
    {
        private readonly string _tag;
        private readonly ILogger _logger;

#pragma warning disable 618
        public TypedLogger()
        {
            _tag = typeof(T).ToString();
#if UNITY_2018_1_OR_NEWER
            _logger = Debug.unityLogger;
#else
			_logger = Debug.logger;
#endif
        }

        public TypedLogger(string tag)
        {
            _tag = tag;
#if UNITY_2018_1_OR_NEWER
            _logger = Debug.unityLogger;
#else
			_logger = Debug.logger;
#endif
        }

#pragma warning restore 618
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            _logger.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, Object context)
        {
            _logger.LogException(exception, context);
        }

        public bool IsLogTypeAllowed(LogType logType)
        {
            return _logger.IsLogTypeAllowed(logType);
        }

        public void Log(LogType logType, object message)
        {
            _logger.Log(logType, _tag, message);
        }

        public void Log(LogType logType, object message, Object context)
        {
            _logger.Log(logType, _tag, message, context);
        }

        public void Log(object message)
        {
            _logger.Log(_tag, message);
        }
        
        public void LogWarning(object message)
        {
            _logger.Log(LogType.Warning, _tag, message);
        }

        public void LogError(object message)
        {
            _logger.Log(LogType.Error, _tag, message);
        }
        
        public void LogFormat(LogType logType, string format, params object[] args)
        {
            _logger.LogFormat(logType, _tag, format, args);
        }

        public void LogException(Exception exception)
        {
            _logger.LogException(exception);
        }
    }
#endif
}