using System;

namespace PestelLib.ServerCommon.Db
{
    public interface IUserMaintenance
    {
        DateTime GetUserMaintenanceEnd(Guid userId);
    }

    public interface IUserMaintenancePrivate
    {
        void SetUserMaintenance(Guid userId, DateTime untilDate);
        long RemoveBeforeDate(DateTime excludeDate);
        void Remove(Guid userId);
    }
}
