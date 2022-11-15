using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject()]
    public class QuestEventsState
    {
        [Key(0)]
        public int WeekIndex = 0;

        [Key(1)]
        public bool CliamReward = false;

        [Key(2)]
        public int[] UnclamedEvent;
    }
}