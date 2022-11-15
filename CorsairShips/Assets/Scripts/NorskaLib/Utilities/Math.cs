using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace NorskaLib.Utilities
{ 
    public struct MathUtils
    {
        public static int[] GetRange(int min, int max, int[] exeptions)
        {
            var pool = new List<int>();
            for (int i = min; i < max; i++)
            {
                var exeption = false;
                for (int j = 0; j < exeptions.Length; j++)
                    if (i == exeptions[j])
                    {
                        exeption = true;
                        break;
                    }

                if (!exeption)
                    pool.Add(i);
            }

           return  pool.ToArray();
        }

        public static float Radians(float degrees)
        {
            return degrees * Mathf.Deg2Rad;
        }

        /// <summary>
        /// Отображает угол в диапазоне [-180; 180] в диапазон [0; 360]
        /// </summary>
        public static float SignedTo360(float signedAngle)
        {
            return signedAngle < 0
                ? 360 + signedAngle
                : signedAngle;
        }

        public static Vector3 PointOnCylindricSpiral(Vector3 origin, float radius, float height, float t, bool clockwise = true, float initialAngle = 0)
        {
            var sign = clockwise
                ? +1
                : -1;
            var p = (sign * t * 2 + initialAngle / 180) * Mathf.PI;

            var x = radius * Mathf.Sin(p);
            var y = height * t;
            var z = radius * Mathf.Cos(p);

            return origin + new Vector3(x, y, z);
        }

        public static float CylindricSpiralLength(float R, float H)
        {
            var b = H / (2 * Mathf.PI);
            return 2 * Mathf.PI * Mathf.Sqrt(R * R + b * b);
        }

        public static float CircleLength(float radius)
        {
            return 2 * Mathf.PI * radius;
        }

        public static float ArcLength(float radius, float degrees)
        {
            var p = degrees * Mathf.PI / 180;
            return p * radius;
        }

        public static Vector3 Project(Vector3 position, Vector3 A, Vector3 B)
        {
            var normal = (B - A).normalized;
            var vector = position - A;

            return A + Vector3.Project(vector, normal);
        }

        public static Vector3 PositionOnCircle3D(Vector3 origin, float degrees, float radius)
        {
            var p = degrees * Mathf.PI / 180;

            var z = radius * Mathf.Cos(p);
            var x = radius * Mathf.Sin(p);

            return origin + new Vector3(x, 0, z);
        }
        public static Vector2 PositionOnCircle2D(Vector2 origin, float degrees, float radius)
        {
            var p = degrees * Mathf.PI / 180;

            var y = radius * Mathf.Cos(p);
            var x = radius * Mathf.Sin(p);

            return origin + new Vector2(x, y);
        }

        public static Vector3 RandomPositionOnCircle(Vector3 origin, float radius)
        {
            var degrees = Random.Range(-180f, 180f);
            return PositionOnCircle3D(origin, degrees, radius);
        }

        public static Vector3 RandomPointOnArc(Transform axis, float r, float d)
        {
            var a = Random.Range(-d/2, d/2);
            var p = a * Mathf.PI / 180;

            var z = r * Mathf.Cos(p);
            var x = r * Mathf.Sin(p);

            return axis.position + axis.forward * z + axis.right * x;
        }

        ///https://ru.wikipedia.org/wiki/Кривая_Безье
        /// <summary>
        /// Возвращает точку на квадратичной кривой Безье.
        /// </summary>
        /// <param name="startPos"> Начальная точка </param>
        /// <param name="handlePos"> Опорная точка, задающая форму </param>
        /// <param name="endPos"> Конечная точка </param>
        /// <param name="t"> Параметр, определяющий долю от пути по кривой, 
        /// на которой находится искомая точка; лежит в пределах от 0 до 1 </param>
        public static Vector3 PointOnQuadCurve(Vector3 startPos, Vector3 handlePos, Vector3 endPos, float t)
        {
            var b = (1 - t) * (1 - t) * startPos
                + 2 * t * (1 - t) * handlePos
                + t * t * endPos;

            return b;
        }
        public static Vector3 PointOnQuadCurveClamped(Vector3 startPos, Vector3 handlePos, Vector3 endPos, float t)
        {
            t = Mathf.Clamp01(t);

            var b = (1 - t) * (1 - t) * startPos
                + 2 * t * (1 - t) * handlePos
                + t * t * endPos;

            return b;
        }
        /// <summary>
        /// Возвращает примерную длину квадратичной кривой Безье.
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="handlePos"></param>
        /// <param name="endPos"></param>
        /// <param name="divCount"> Кол-во отрезков, на которые разбивается кривая. </param>
        /// <returns></returns>
        public static float QuadCurveLengthSlow(Vector3 startPos, Vector3 handlePos, Vector3 endPos, int divCount)
        {
            var length = 0f;
            var a = startPos;
            for (int i = 0; i < divCount; i++)
            {
                var t = (i + 1) / (float)divCount;
                var b = PointOnQuadCurve(startPos, handlePos, endPos, t);
                length += Vector3.Distance(a, b);
                a = b;
            }

            return length;
        }

        /// <summary>
        /// Возвращает точку, на которую смотрит точка на квадратичной кривой
        /// </summary>
        /// <param name="p0"> Начальная точка </param>
        /// <param name="p1"> Опорная точка, задающая форму </param>
        /// <param name="p2"> Конечная точка </param>
        /// <param name="t"> Параметр, определяющий долю от пути по кривой, 
        /// на которой находится точка; лежит в пределах от 0 до 1 </param>
        public static Vector3 LookPositionOnQuad(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return p1 + (p2 - p1) * t;
        }

        // By DylanW https://answers.unity.com/questions/556480/rotate-the-shortest-way.html
        public static float ShortestRotationAngle(float from, float to)
        {
            // If from or to is a negative, we have to recalculate them.
            // For an example, if from = -45 then from(-45) + 360 = 315.
            if (from < 0)
            {
                from += 360;
            }

            if (to < 0)
            {
                to += 360;
            }

            // Do not rotate if from == to.
            if (from == to ||
               from == 0 && to == 360 ||
               from == 360 && to == 0)
            {
                return 0;
            }

            // Pre-calculate left and right.
            float left = (360 - from) + to;
            float right = from - to;
            // If from < to, re-calculate left and right.
            if (from < to)
            {
                if (to > 0)
                {
                    left = to - from;
                    right = (360 - to) + from;
                }
                else
                {
                    left = (360 - to) + from;
                    right = to - from;
                }
            }

            // Determine the shortest direction.
            return (left <= right) 
                ? left 
                : (right * -1);
        }

        public static int ShortestRotationDirection(float from, float to)
        {
            var angle = ShortestRotationAngle(from, to);
            if (angle > 0)
                return 1;
            else if (angle < 0)
                return -1;
            else
                return 0;
        }

        /// <summary>
        /// Возвращает угол в плоскости XZ между указанным Transform'ом и точкой;
        /// осью отсчета считается вектор origin.forward, отображенный в плоскость XZ
        /// </summary>
        public static float RelativeSignedAngleXZ(Transform origin, Vector3 position)
        {
            var origin2D = new Vector2(origin.position.x, origin.position.z);
            var axis2D = new Vector2(origin.forward.x, origin.forward.z);
            var position2D = new Vector2(position.x, position.z);
            var direction2D = position2D - origin2D;

            return Vector2.SignedAngle(axis2D, direction2D);
        }
        public static float RelativeSignedAngleXZ(Vector3 origin, Vector3 positionA, Vector3 positionB)
        {
            var origin2D = new Vector2(origin.x, origin.z);
            var directionA2D = new Vector2(positionA.x, positionA.z) - origin2D;
            var directionB2D = new Vector2(positionB.x, positionB.z) - origin2D;

            return Vector2.SignedAngle(directionA2D, directionB2D);
        }
        public static float RelativeUnsignedAngleXZ(Vector3 directionnA, Vector3 directionB)
        {
            var directionA2D = new Vector2(directionnA.x, directionnA.z);
            var directionB2D = new Vector2(directionB.x, directionB.z);

            return Vector2.Angle(directionA2D, directionB2D);
        }

        /// <summary>
        /// Возвращает угол в плоскости XZ между указанной точкой и точкой;
        /// осью отсчета считается вектор Vector.forward (мировой Север), отображенный в плоскость XZ
        /// </summary>
        public static float AbsoluteSignedAngleXZ(Vector3 origin, Vector3 position)
        {
            var origin2D = new Vector2(origin.x, origin.z);
            var axis2D = new Vector2(Vector3.forward.x, Vector3.forward.z);
            var position2D = new Vector2(position.x, position.z);
            var direction2D = position2D - origin2D;

            return -Vector2.SignedAngle(axis2D, direction2D);
        }
        public static float AbsoluteSignedAngleXZ(Vector3 direction)
        {
            var axis2D = new Vector2(Vector3.forward.x, Vector3.forward.z);
            var direction2D = new Vector2(direction.x, direction.z);

            return -Vector2.SignedAngle(axis2D, direction2D);
        }
        public static float AbsoluteSignedAngleXZ(Vector2 direction)
        {
            var axis2D = new Vector2(Vector3.forward.x, Vector3.forward.z);

            return -Vector2.SignedAngle(axis2D, direction);
        }

        /// <summary>
        /// Возвращает угол в плоскости XZ между вектором forward указанного Transform'а
        /// и осью отсчета, которой считается вектор Vector.forward (мировой Север)
        /// </summary>
        public static float FacingSignedAngleXZ(Transform transform)
        {
            var forward2D = new Vector2(transform.forward.x, transform.forward.z);
            var axis2D = new Vector2(Vector3.forward.x, Vector3.forward.z);

            return -Vector2.SignedAngle(axis2D, forward2D);
        }
    
        /// <summary>
        /// Returns a Vector3 in XZ plane, pointing towards given angle. 0 degrees considered Vector3.forward (0, 0, 1).
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static Vector3 AngleToVector3XZ(float degrees)
        {
            return PositionOnCircle3D(Vector3.zero, degrees, 1);
        }
        /// <summary>
        /// Returns a Vector2, pointing towards given angle. 0 degrees considered Vector2.up (0, 1).
        /// </summary>
        public static Vector2 AngleToVector2(float degrees)
        {
            return PositionOnCircle2D(Vector2.zero, degrees, 1);
        }
    }
}
