using UnityEngine;

namespace UnityDI.Finders
{
	/// <summary>
	/// Интерфейс класса, ищущего игровые объекты по пути
	/// </summary>
	public interface IUnityGameObjectFinder
	{
		GameObject Find(string path);
	}
}
