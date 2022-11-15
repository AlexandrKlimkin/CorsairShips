using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PestelLib.ServerCommon.Db;
using PestelLib.ServerShared;
using S;
using ServerLib.Modules.ServerMessages;

namespace BackendCommon.Code.IapValidator
{
    public class IapValidatorHub : IIapValidator
    {
        private Dictionary<string, IIapValidator> _handlers;

        public IapValidatorHub(bool fakePaymentsAllowed)
        {
            _handlers = new Dictionary<string, IIapValidator>
            {
                { "ios", new IosIapValidator() },
                { "android", AndroidIapValidator.Create() }
            };

            if (fakePaymentsAllowed)
            {
                _handlers["fake"] = new FakeIappValidator();
            }
        }

        public IapValidateResult IsValid(IapValidateQuery query, bool acceptOnFail)
        {
            return IsValidAsync(query, acceptOnFail).Result;
        }

        public async Task<IapValidateResult> IsValidAsync(IapValidateQuery query, bool acceptOnFail)
        {
            if (!_handlers.TryGetValue(query.Platform, out var handler))
                throw new NotSupportedException(string.Format((string) "Platform '{0}' not supported", (object) query.Platform));

            var result = await handler.IsValidAsync(query, acceptOnFail).ConfigureAwait(false);

            var resultJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            var resultBytes = Encoding.UTF8.GetBytes(resultJson);

            ServerMessageUtils.SendMessage(new ServerMessage()
            {
                MessageType = typeof(IapValidateResult).Name,
                Data = resultBytes
            }, query.PlayerId);

            return result;
        }
    }
}
