using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer
{
    internal interface ISuperUsersDb
    {
        bool IsSuper(byte[] authData);
        bool AddSuper(byte[] authData);
        bool RemoveSuper(byte[] authData);
    }
}
