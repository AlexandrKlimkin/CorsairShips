using System;

namespace UnityDI.Providers
{
	public class ActivatorObjectProvider<T> : IObjectProvider<T> where T : class, new()
	{
		public T GetObject(Container container)
		{
			var obj = Activator.CreateInstance<T>();
			container.BuildUp<T>(obj);
			return obj;
		}
	}
}
