using System;
using PestelLib.ServerShared;
using PestelLib.ServerCommon.Db;
using S;

namespace ServerLib.Modules
{
    public class SendFeedbackModule : IModule
    {
        public SendFeedbackModule(IServiceProvider serviceProvider)
        {
            _feedbackStorage = serviceProvider.GetService(typeof(IFeedbackStorage)) as IFeedbackStorage;
        }

        public ServerResponse ProcessCommand(ServerRequest request)
        {
            var r = request.Request.SendFeedback;

            _feedbackStorage.Save(r, DateTime.UtcNow);

            return new ServerResponse { ResponseCode = ResponseCode.OK };
        }

        private IFeedbackStorage _feedbackStorage;
    }
}