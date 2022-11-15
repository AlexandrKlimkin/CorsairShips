using System.Reflection;

namespace UnityDI
{
    public struct CachedDependency
    {
        public FieldInfo Field;
        public DependencyAttribute Attribute;
    }
}