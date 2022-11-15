using UnityEngine;

namespace PestelLib.Utils
{
    public class ColorUnitls
    {
        public static bool HexToColor(string h, out Color c)
        {
            c = Color.white;

            if (h == null)
                h = "0";
            if (h.Length > 0 && h[0] == '#')
                h = h.Substring(1);
            if (h.Length > 1 && h[0] == '[' && h[h.Length - 1] == ']')
                h = h.Substring(1, h.Length - 2);
            if (h.Length > 8)
                h = h.Substring(0, 8);
            while (h.Length < 6)
                h += "0";
            if (h.Length > 6)
                while (h.Length < 8)
                    h += "0";

            int color = 0;
            if (!int.TryParse(h, System.Globalization.NumberStyles.HexNumber, null, out color))
                return false;
            var b0 = (color >> 24) & 0xff;
            var b1 = (color >> 16) & 0xff;
            var b2 = (color >> 8) & 0xff;
            var b3 = (color) & 0xff;


            if (h.Length == 6)
                c = new Color(b1 / 255.0f, b2 / 255.0f, b3 / 255.0f, 1.0f);
            else
                c = new Color(b0 / 255.0f, b1 / 255.0f, b2 / 255.0f, b3 / 255.0f);
            return true;
        }

        public static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }
    }
}