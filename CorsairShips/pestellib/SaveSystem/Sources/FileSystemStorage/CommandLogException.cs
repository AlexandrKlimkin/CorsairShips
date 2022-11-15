using System;

namespace PestelLib.SaveSystem.FileSystemStorage
{
    public class CommandLogException : Exception
    {
        public enum ExceptionType
        {
            WrongCRC
        }

        public ExceptionType Type { get; private set; }

        public CommandLogException(ExceptionType type)
        {
            Type = type;
        }
    }
}