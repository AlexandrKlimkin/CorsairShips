using System;

namespace PestelLib.SharedLogicBase
{
    public class SharedLogicException : Exception
    {
        public enum SharedLogicExceptionType
        {
            WRONG_COMMANDS_ORDER,
            LOST_COMMANDS,
            PROMO_CODE_ERROR
        }

        public SharedLogicExceptionType ExceptionType { get; private set; }

        public SharedLogicException(SharedLogicExceptionType exceptionType, string message) : base(message)
        {
            ExceptionType = exceptionType;
        }
    }
}
