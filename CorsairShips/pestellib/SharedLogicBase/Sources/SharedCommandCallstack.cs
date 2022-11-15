using System;
using System.Diagnostics;
using ServerShared;

namespace PestelLib.SharedLogicBase
{
    public class SharedCommandCallstack
    {
        /*
         * Позволяет убедиться, что метод не вызван из клиентского кода напрямую.
         */
        [Conditional("UNITY_EDITOR")]
        public static void CheckCallstack()
        {
            var stacktrace = Environment.StackTrace;
            UniversalAssert.IsTrue(
                stacktrace.Contains("PestelLib.SharedLogicBase.SharedLogicDefault") && (stacktrace.Contains(".MakeDefaultState") || stacktrace.Contains(".Process")), 
                "<b>You should't change state from client directly! Please use CommandHelper.cs</b>");
        }

        public static bool CheckProfileViewer()
        {
            var stacktrace = Environment.StackTrace;
            return stacktrace.Contains("UserProfileViewerEditor");
        }
    }
}