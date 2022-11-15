using System;

namespace PestelLib.SharedLogicBase
{
    public struct PestelMathf
    {
        public static readonly float Epsilon = float.Epsilon;
        public const float PI = 3.141593f;
        public const float Infinity = float.PositiveInfinity;
        public const float NegativeInfinity = float.NegativeInfinity;
        public const float Deg2Rad = 0.01745329f;
        public const float Rad2Deg = 57.29578f;

        public static float Sin(float f)
        {
            return (float)Math.Sin((double)f);
        }

        public static float Cos(float f)
        {
            return (float)Math.Cos((double)f);
        }

        public static float Tan(float f)
        {
            return (float)Math.Tan((double)f);
        }

        public static float Asin(float f)
        {
            return (float)Math.Asin((double)f);
        }

        public static float Acos(float f)
        {
            return (float)Math.Acos((double)f);
        }

        public static float Atan(float f)
        {
            return (float)Math.Atan((double)f);
        }

        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2((double)y, (double)x);
        }

        public static float Sqrt(float f)
        {
            return (float)Math.Sqrt((double)f);
        }

        public static float Abs(float f)
        {
            return Math.Abs(f);
        }

        public static int Abs(int value)
        {
            return Math.Abs(value);
        }

        public static float Min(float a, float b)
        {
            if ((double)a < (double)b)
                return a;
            else
                return b;
        }

        public static float Min(params float[] values)
        {
            int length = values.Length;
            if (length == 0)
                return 0.0f;
            float num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if ((double)values[index] < (double)num)
                    num = values[index];
            }
            return num;
        }

        public static int Min(int a, int b)
        {
            if (a < b)
                return a;
            else
                return b;
        }

        public static int Min(params int[] values)
        {
            int length = values.Length;
            if (length == 0)
                return 0;
            int num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if (values[index] < num)
                    num = values[index];
            }
            return num;
        }

        public static float Max(float a, float b)
        {
            if ((double)a > (double)b)
                return a;
            else
                return b;
        }

        public static float Max(params float[] values)
        {
            int length = values.Length;
            if (length == 0)
                return 0.0f;
            float num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if ((double)values[index] > (double)num)
                    num = values[index];
            }
            return num;
        }

        public static int Max(int a, int b)
        {
            if (a > b)
                return a;
            else
                return b;
        }

        public static int Max(params int[] values)
        {
            int length = values.Length;
            if (length == 0)
                return 0;
            int num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if (values[index] > num)
                    num = values[index];
            }
            return num;
        }

        public static float Pow(float f, float p)
        {
            return (float)Math.Pow((double)f, (double)p);
        }

        public static float Exp(float power)
        {
            return (float)Math.Exp((double)power);
        }

        public static float Log(float f, float p)
        {
            return (float)Math.Log((double)f, (double)p);
        }

        public static float Log(float f)
        {
            return (float)Math.Log((double)f);
        }

        public static float Log10(float f)
        {
            return (float)Math.Log10((double)f);
        }

        public static float Ceil(float f)
        {
            return (float)Math.Ceiling((double)f);
        }

        public static float Floor(float f)
        {
            return (float)Math.Floor((double)f);
        }

        public static float Round(float f)
        {
            return (float)Math.Round((double)f);
        }

        public static int RoundToEven(decimal d)
        {
            return (int)Math.Round(d, MidpointRounding.ToEven);
        }

        public static int CeilToInt(float f)
        {
            return (int)Math.Ceiling((double)f);
        }

        public static int CeilToInt(decimal d)
        {
            return (int)Math.Ceiling(d);
        }

        public static int FloorToInt(float f)
        {
            return (int)Math.Floor((double)f);
        }

        public static int RoundToInt(float f)
        {
            return (int)Math.Round((double)f);
        }

        public static int RoundToInt(decimal d)
        {
            return (int)Math.Round(d);
        }

        public static float Sign(float f)
        {
            return (double)f >= 0.0 ? 1f : -1f;
        }

        public static float Clamp(float value, float min, float max)
        {
            if ((double)value < (double)min)
                value = min;
            else if ((double)value > (double)max)
                value = max;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        public static float Clamp01(float value)
        {
            if ((double)value < 0.0)
                return 0.0f;
            if ((double)value > 1.0)
                return 1f;
            else
                return value;
        }

        public static float Lerp(float from, float to, float t)
        {
            return from + (to - from) * Clamp01(t);
        }

        public static float LerpAngle(float a, float b, float t)
        {
            float num = Repeat(b - a, 360f);
            if ((double)num > 180.0)
                num -= 360f;
            return a + num * Clamp01(t);
        }

        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if ((double)Abs(target - current) <= (double)maxDelta)
                return target;
            else
                return current + Sign(target - current) * maxDelta;
        }

        public static float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            target = current + DeltaAngle(current, target);
            return MoveTowards(current, target, maxDelta);
        }

        public static float SmoothStep(float from, float to, float t)
        {
            t = Clamp01(t);
            t = (float)(-2.0 * (double)t * (double)t * (double)t + 3.0 * (double)t * (double)t);
            return (float)((double)to * (double)t + (double)from * (1.0 - (double)t));
        }

        public static float Gamma(float value, float absmax, float gamma)
        {
            bool flag = false;
            if ((double)value < 0.0)
                flag = true;
            float num1 = Abs(value);
            if ((double)num1 > (double)absmax)
            {
                if (flag)
                    return -num1;
                else
                    return num1;
            }
            else
            {
                float num2 = Pow(num1 / absmax, gamma) * absmax;
                if (flag)
                    return -num2;
                else
                    return num2;
            }
        }

        public static bool Approximately(float a, float b)
        {
            return (double)Abs(b - a) < (double)Max(1E-06f * Max(Abs(a), Abs(b)), Epsilon * 8f);
        }

        /*
        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed)
        {
            float deltaTime = Time.deltaTime;
            return Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime)
        {
            float deltaTime = Time.deltaTime;
            float maxSpeed = float.PositiveInfinity;
            return Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime)
        {
            smoothTime = Mathf.Max(0.0001f, smoothTime);
            float num1 = 2f / smoothTime;
            float num2 = num1 * deltaTime;
            float num3 = (float)(1.0 / (1.0 + (double)num2 + 0.479999989271164 * (double)num2 * (double)num2 + 0.234999999403954 * (double)num2 * (double)num2 * (double)num2));
            float num4 = current - target;
            float num5 = target;
            float max = maxSpeed * smoothTime;
            float num6 = Mathf.Clamp(num4, -max, max);
            target = current - num6;
            float num7 = (currentVelocity + num1 * num6) * deltaTime;
            currentVelocity = (currentVelocity - num1 * num7) * num3;
            float num8 = target + (num6 + num7) * num3;
            if ((double)num5 - (double)current > 0.0 == (double)num8 > (double)num5)
            {
                num8 = num5;
                currentVelocity = (num8 - num5) / deltaTime;
            }
            return num8;
        }

        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed)
        {
            float deltaTime = Time.deltaTime;
            return Mathf.SmoothDampAngle(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime)
        {
            float deltaTime = Time.deltaTime;
            float maxSpeed = float.PositiveInfinity;
            return Mathf.SmoothDampAngle(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime)
        {
            target = current + Mathf.DeltaAngle(current, target);
            return Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }*/

        public static float Repeat(float t, float length)
        {
            return t - Floor(t / length) * length;
        }

        public static float PingPong(float t, float length)
        {
            t = Repeat(t, length * 2f);
            return length - Abs(t - length);
        }

        public static float InverseLerp(float from, float to, float value)
        {
            if ((double)from < (double)to)
            {
                if ((double)value < (double)from)
                    return 0.0f;
                if ((double)value > (double)to)
                    return 1f;
                value -= from;
                value /= to - from;
                return value;
            }
            else
            {
                if ((double)from <= (double)to)
                    return 0.0f;
                if ((double)value < (double)to)
                    return 1f;
                if ((double)value > (double)from)
                    return 0.0f;
                else
                    return (float)(1.0 - ((double)value - (double)to) / ((double)from - (double)to));
            }
        }

        public static float DeltaAngle(float current, float target)
        {
            float num = Repeat(target - current, 360f);
            if ((double)num > 180.0)
                num -= 360f;
            return num;
        }

        /*
        internal static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 result)
        {
            float num1 = p2.x - p1.x;
            float num2 = p2.y - p1.y;
            float num3 = p4.x - p3.x;
            float num4 = p4.y - p3.y;
            float num5 = (float)((double)num1 * (double)num4 - (double)num2 * (double)num3);
            if ((double)num5 == 0.0)
                return false;
            float num6 = p3.x - p1.x;
            float num7 = p3.y - p1.y;
            float num8 = (float)((double)num6 * (double)num4 - (double)num7 * (double)num3) / num5;
            result = new Vector2(p1.x + num8 * num1, p1.y + num8 * num2);
            return true;
        }

        internal static bool LineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 result)
        {
            float num1 = p2.x - p1.x;
            float num2 = p2.y - p1.y;
            float num3 = p4.x - p3.x;
            float num4 = p4.y - p3.y;
            float num5 = (float)((double)num1 * (double)num4 - (double)num2 * (double)num3);
            if ((double)num5 == 0.0)
                return false;
            float num6 = p3.x - p1.x;
            float num7 = p3.y - p1.y;
            float num8 = (float)((double)num6 * (double)num4 - (double)num7 * (double)num3) / num5;
            if ((double)num8 < 0.0 || (double)num8 > 1.0)
                return false;
            float num9 = (float)((double)num6 * (double)num2 - (double)num7 * (double)num1) / num5;
            if ((double)num9 < 0.0 || (double)num9 > 1.0)
                return false;
            result = new Vector2(p1.x + num8 * num1, p1.y + num8 * num2);
            return true;
        }*/

        internal static long RandomToLong(System.Random r)
        {
            byte[] buffer = new byte[8];
            r.NextBytes(buffer);
            return (long)BitConverter.ToUInt64(buffer, 0) & long.MaxValue;
        }
    }
}
