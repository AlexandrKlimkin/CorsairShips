using System;

namespace ServerShared
{
    public class AbTesting
    {
        public static bool IsInExperimentGroup(byte[] playerId)
        {
            return playerId[0] % 2 == 0;
        }

        public static bool IsInExperimentGroup(Guid playerId)
        {
            return IsInExperimentGroup(playerId.ToByteArray());
        }
    }
}
