using PestelLib.ServerShared;

namespace PestelLib.ServerShared
{
    public class CommandCode
    {
        public static int CodeByName(string cmdName)
        {
            return (int)Crc32.Compute(cmdName);
        }
    }
}