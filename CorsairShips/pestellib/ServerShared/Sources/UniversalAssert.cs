using System.Diagnostics;

namespace ServerShared
{
    public class UniversalAssert
    {
        [Conditional("UNITY_ASSERTIONS")]
        public static void IsTrue(bool condition)
        {
#if UNITY_5_5_OR_NEWER
            UnityEngine.Assertions.Assert.IsTrue(condition);
#else
            Debug.Assert(condition);
#endif
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsTrue(bool condition, string message)
        {
#if UNITY_5_5_OR_NEWER
            UnityEngine.Assertions.Assert.IsTrue(condition, message);
#else
            Debug.Assert(condition, message);
#endif
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsNotNull(object value)
        {
#if UNITY_5_5_OR_NEWER
            UnityEngine.Assertions.Assert.IsNotNull(value);
#else
            Debug.Assert(value != null);
#endif
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsNotNull(object value, string message)
        {
#if UNITY_5_5_OR_NEWER
            UnityEngine.Assertions.Assert.IsNotNull(value, message);
#else
            Debug.Assert(value != null, message);
#endif
        }
    }
}