using System;
using System.Collections.Generic;
using System.Text;

namespace PestelLib.ServerCommon.Db
{
    interface IApiFactory
    {
        IMatchInfo GetMatchInfoApi();
    }
}
