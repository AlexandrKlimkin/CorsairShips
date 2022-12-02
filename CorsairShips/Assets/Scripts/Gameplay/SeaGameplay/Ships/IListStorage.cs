using System.Collections.Generic;

namespace Game.SeaGameplay {
    public interface IListStorage<T> {
        public List<T> Storage { get; }
    }
}