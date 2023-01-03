using System;
using System.Collections.Generic;

namespace Game.SeaGameplay.Points {
    public interface IPointsCounter {
        
        Dictionary<byte, int> PointsDict { get; }
        event Action<byte, int> OnPointsChanged;
    }
}