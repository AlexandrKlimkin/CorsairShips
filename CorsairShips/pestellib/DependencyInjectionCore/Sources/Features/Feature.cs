namespace UnityDI
{
    public class Feature<TFeature> : IFeature where TFeature: class 
    {
        private readonly TFeature _feature;

        public Feature(TFeature feature)
        {
            _feature = feature;
        }

        public void Visit(Container container)
        {
            container.RegisterInstance(_feature);
        }
    }
}
