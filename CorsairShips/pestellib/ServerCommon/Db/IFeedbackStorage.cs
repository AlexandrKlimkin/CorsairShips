using System;
using S;

namespace PestelLib.ServerCommon.Db
{
    public class FeedbackStorageItem
    {
        public DateTime RegDate;
        public SendFeedback Feedback;
    }

    public interface IFeedbackStorage
    {
        void Save(SendFeedback feedback, DateTime regDate);
        long Count();
        long Count(DateTime from, DateTime to);
        FeedbackStorageItem[] GetRange(DateTime from, DateTime to);
    }
}
