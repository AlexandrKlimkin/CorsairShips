namespace PestelLib.ClientConfig
{
    [System.Serializable]
    public class PhotonServerDescription
    {
        public string Name;
        public string Uri;
        public int Port;

        public override string ToString()
        {
            return Uri;
        }
    }
}