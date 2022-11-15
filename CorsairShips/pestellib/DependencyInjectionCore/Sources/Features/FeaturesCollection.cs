using System.Collections.Generic;

namespace UnityDI
{
    public class FeaturesCollection : IFeature
    {
        private List<IFeature> _features = new List<IFeature>();

        public void AddFeature(IFeature feature)
        {
            _features.Add(feature);
        }

        public void Visit(Container container)
        {
            for (var i = 0; i < _features.Count; i++)
            {
                _features[i].Visit(container);
            }
        }
    }
}
