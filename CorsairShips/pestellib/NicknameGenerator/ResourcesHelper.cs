using System.IO;
using System.Reflection;

namespace PestelLib
{
    public class ResourcesHelper
    {
        public static string GetEmbeddedResource(Assembly assembly, string resourceName)
        {
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    return null;

                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
