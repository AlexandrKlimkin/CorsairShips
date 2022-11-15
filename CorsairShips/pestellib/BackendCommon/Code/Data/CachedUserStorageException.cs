using System;

namespace BackendCommon.Code.Data
{
    public class UserStorageException : Exception
    {
        public enum ErrorEnum
        {
            STORAGE_NOT_AVAILABLE,
            SAVE_RAW_DATA_FAILED
        }

        public ErrorEnum Error { get; private set; }
        public Guid? Guid { get; private set; }

        public UserStorageException(ErrorEnum error, Guid? guid)
        {
            Error = error;
            Guid = guid;
        }
    }
}