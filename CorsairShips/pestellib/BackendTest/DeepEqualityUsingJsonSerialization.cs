using System;
using System.IO;
using System.Runtime.Serialization.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public class DeepEqualityUsingJsonSerialization
{
    public static void ShouldDeepEqual<T>(T expected, T actual)
    {
        Assert.IsInstanceOfType(actual, expected.GetType());
        var serializedExpected = Serialize(expected);
        var serializedActual = Serialize(actual);
        Assert.AreEqual(serializedExpected, serializedActual);
    }

    public static bool IsDeepEqual<T>(T expected, T actual)
    {
        var serializedExpected = Serialize(expected);
        var serializedActual = Serialize(actual);
        return serializedExpected == serializedActual;
    }

    public static string Serialize<T>(T obj)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        try
        {
            var stream1 = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(stream1, obj);
            stream1.Position = 0;
            var streamReader = new StreamReader(stream1);
            return streamReader.ReadToEnd();
        }
        catch
        {
            return string.Empty;
        }
    }
}
