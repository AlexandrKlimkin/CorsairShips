using System.Collections.Generic;
using System.Linq;

namespace ServerShared.GlobalConflict
{
    public enum MessageLevel
    {
        Error,
        Warning
    }
    public struct ValidatorMesage
    {
        public MessageLevel Level;
        public string Message;
    }

    public class ValidatorMessageCollection : List<ValidatorMesage>
    {
        public int Errors
        {
            get
            {
                return CountByLevel(MessageLevel.Error);
            }
        }
        public int Warnings
        {
            get
            {
                return CountByLevel(MessageLevel.Warning);
            }
        }
        public void Add(MessageLevel level, string message)
        {
            Add(new ValidatorMesage { Level = level, Message = message });
        }

        private int CountByLevel(MessageLevel lvl)
        {
            return this.Count(_ => _.Level == lvl);
        }
    }

    public interface IGlobalConflictValidator
    {
        bool IsValid(GlobalConflictState state, ValidatorMessageCollection messages);
    }

    
}
