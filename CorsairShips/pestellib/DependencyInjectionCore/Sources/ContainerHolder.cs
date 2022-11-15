namespace UnityDI
{
    public class ContainerHolder
    {
        private static Container _container;
        public static Container Container {
            get
            {
                if (_container == null)
                {
                    _container = new Container();
                }
                return _container;
            }

            set { _container = value; }
        }
    }
}
