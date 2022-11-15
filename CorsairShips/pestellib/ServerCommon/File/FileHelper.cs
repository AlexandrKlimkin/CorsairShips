using System.IO;

namespace PestelLib.ServerCommon
{
    public static class FileHelper
    {
        public static void WriteFile(string fileName, string data)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                using (var fs = System.IO.File.Open(path, FileMode.Create))
                {
                    using (var tw = new StreamWriter(fs))
                    {
                        tw.Write(data);
                    }
                }
            }
            catch
            {
            }
        }
    }
}
