using System.Collections.Generic;
using UnityEngine;

namespace PestelLib.Utils
{
    static public class TimeSwitcher
    {
        public enum TimeType
        {
            Game,
            Pause,
            System
        };

        static private Dictionary<TimeType, float> _scales = new Dictionary<TimeType, float>();

        static public void SetTimescale(TimeType id, float scale)
        {
            _scales[id] = scale;

            Time.timeScale = 1;
            foreach (var s in _scales.Values)
            {
                Time.timeScale *= s;
            }
        }

        static public float GetTimescale(TimeType id)
        {
            return _scales[id];
        }
    }
}
