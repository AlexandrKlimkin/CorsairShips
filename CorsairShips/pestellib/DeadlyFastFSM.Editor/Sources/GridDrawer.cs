using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DeadlyFast
{
    public class GridDrawer
    {
        private static readonly Color kGridMinorColorDark = new Color(0.0f, 0.0f, 0.0f, 0.18f);
        private static readonly Color kGridMajorColorDark = new Color(0.0f, 0.0f, 0.0f, 0.28f);
        private static readonly Color kGridMinorColorLight = new Color(0.0f, 0.0f, 0.0f, 0.1f);
        private static readonly Color kGridMajorColorLight = new Color(0.0f, 0.0f, 0.0f, 0.15f);

        public static void DrawGrid(Vector2 Min, Vector2 Max)
        {
            if (Event.current != null && Event.current.type != EventType.Repaint)
                return;

            var method = typeof(HandleUtility).GetMethod("ApplyWireMaterial", 
                BindingFlags.NonPublic | BindingFlags.Static, 
                null,
                CallingConventions.Standard, 
                new Type[] { }, 
                new ParameterModifier[] { }
            );

            if (method != null)
            {
                method.Invoke(null, null);

                GL.PushMatrix();
                GL.Begin(1);
                DrawGridLines(12f, gridMinorColor, Min, Max);
                DrawGridLines(120f, gridMajorColor, Min, Max);
                GL.End();
                GL.PopMatrix();
            }
        }

        private static Color gridMinorColor
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                    return kGridMinorColorDark;
                return kGridMinorColorLight;
            }
        }

        private static Color gridMajorColor
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                    return kGridMajorColorDark;
                return kGridMajorColorLight;
            }
        }

        private static void DrawGridLines(float gridSize, Color gridColor, Vector3 Min, Vector3 Max)
        {
            GL.Color(gridColor);
            float x = Min.x - Min.x%gridSize;
            while ((double) x < (double) Max.x)
            {
                DrawLine(new Vector2(x, Min.y), new Vector2(x, Max.y));
                x += gridSize;
            }
            GL.Color(gridColor);
            float y = Min.y - Min.y % gridSize;
            while ((double) y < (double) Max.y)
            {
                DrawLine(new Vector2(Min.x, y), new Vector2(Max.x, y));
                y += gridSize;
            }
        }

        private static void DrawLine(Vector2 p1, Vector2 p2)
        {
            GL.Vertex((Vector3) p1);
            GL.Vertex((Vector3) p2);
        }
    }
}