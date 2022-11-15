using System;

namespace ReportPlayers
{
    public class ReportPlayerException : Exception
    {
        public ReportPlayerException(string message) : base(message)
        {
        }
    }
}