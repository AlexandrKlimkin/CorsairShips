using System;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using PestelLib.ServerShared;

namespace BackendCommon.Code.IapValidator
{
    [HttpContentType(MimeType = "application/json")]
    public class IapValidatorRouteHandler : IRequestHandler
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(IapValidatorRouteHandler));

        private JsonSerializerSettings _settings;
        private bool _acceptOnFail;

        public IIapValidator Validator { get; }

        public IapValidatorRouteHandler(IIapValidator validator, bool acceptOnFail)
        {
            Log.Info("IapValidatorRouteHandler ctor");

            Validator = validator;
            _acceptOnFail = acceptOnFail;
            _settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };
        }

        public async Task<byte[]> Handle(byte[] data)
        {
            IapValidateResult result = new IapValidateResult();
            string body;
            byte[] output;
            try
            {
                body = Encoding.UTF8.GetString(data);
                var query = JsonConvert.DeserializeObject<IapValidateQuery>(body);

                result = await Validator.IsValidAsync(query, _acceptOnFail).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                var json = JsonConvert.SerializeObject(result);
                output = Encoding.UTF8.GetBytes(json);
            }
            return output;
        }

        public Task<byte[]> Process(byte[] data, RequestContext ctx)
        {
            return Handle(data);
        }
    }
}
