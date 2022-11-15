using System.Collections.Generic;
using System.Diagnostics;

public static class SimpleAudioCompression
{
    public static short[] Compress(float[] data)
    {
        var result = Pull(data.Length);
        for (var i = 0; i < data.Length; ++i)
        {
            result[i] = (short)(data[i] * 32767);
        }
        return result;
    }

    public static void Decompress(short[] data, float[] outData)
    {
        Debug.Assert(data.Length <= outData.Length);
        for (var i = 0; i < data.Length; ++i)
        {
            outData[i] = data[i] / 32767f;
        }
    }

    public static void Push(short[] data)
    {
        if (!_bufferPool.TryGetValue(data.Length, out var queue))
        {
            _bufferPool[data.Length] = queue = new Queue<short[]>();
        }
        queue.Enqueue(data);
    }

    private static short[] Pull(int size)
    {
        if (!_bufferPool.TryGetValue(size, out var queue) || queue.Count == 0)
        {
            return new short[size];
        }
        else
        {
            return queue.Dequeue();
        }
    }

    private static Dictionary<int, Queue<short[]>> _bufferPool = new Dictionary<int, Queue<short[]>>();
}
