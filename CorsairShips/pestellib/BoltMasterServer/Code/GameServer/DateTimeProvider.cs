using System;

namespace BoltMasterServer
{
    /// <summary>   Всегда именно так получается текущее время, за исключением тестов. </summary>
    public class DateTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}