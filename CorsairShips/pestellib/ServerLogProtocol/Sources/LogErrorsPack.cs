using System.Collections.Generic;

namespace PestelLib.ServerLogProtocol
{
    public class LogErrorsPack
    {
        public string Game;
        public List<string> Errors = new List<string>();
    }
}