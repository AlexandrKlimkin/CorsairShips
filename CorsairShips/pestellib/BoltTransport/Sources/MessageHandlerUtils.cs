using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoltTransport
{
    public static class MessageHandlerUtils
    {
        private static Dictionary<Type, Dictionary<Type, MethodInfo>> _cache = new Dictionary<Type, Dictionary<Type, MethodInfo>>();

        /// <summary>
        /// Получить все методы-обработчики из заданного типа. Это делает каждый экземпляр подкласса
        /// Connection при своём инстанцировании. Для ускорения результаты кэшируются в _cache т.к. в
        /// одном типе может быть только один набор обработчиков.
        /// </summary>
        ///
        /// <param name="type"> тип, в котором есть обработчики сообщений. </param>
        ///
        /// <returns>   словарь "тип сообщения":"метод-обработчик". </returns>
        public static Dictionary<Type, MethodInfo> GetHandlersFromType(Type type)
        {
            if (_cache.ContainsKey(type)) return _cache[type];
            
            _cache[type] = new Dictionary<Type, MethodInfo>();
            var allMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            foreach (var method in allMethods)
            {
                var attr = method.GetCustomAttributes(typeof(MessageHandlerAttribute), false).FirstOrDefault();
                if (attr is MessageHandlerAttribute)
                {
                    var param = method.GetParameters().FirstOrDefault();
                    if (param != null)
                    {
                        _cache[type][param.ParameterType] = method;
                    }
                }
            }

            return _cache[type];
        }

        /// <summary>
        /// В reflection нет встроенного способа для вызова асинхронного метода, поэтому приходится
        /// использовать этот.
        /// 
        /// Он так же может вызвать синхронный метод, который не возвращает Task (например, у которого
        /// возвращаемое значение void). Т.к. не каждому обработчику нужно возвращать какой-то результат.
        /// </summary>
        ///
        /// <param name="this">         The @this to act on. </param>
        /// <param name="obj">          The object. </param>
        /// <param name="parameters">   A variable-length parameters list containing parameters. </param>
        ///
        /// <returns>   An asynchronous result that yields the invoke. </returns>
        public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            if (@this.Invoke(obj, parameters) is Task task)
            {
                await task.ConfigureAwait(false);
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty.GetValue(task);
            }
            return Task.CompletedTask;
        }
    }
}