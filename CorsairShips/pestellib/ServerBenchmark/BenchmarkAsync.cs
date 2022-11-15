using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using S;
using PestelLib.ServerShared;

namespace ServerBenchmark
{
    public class BenchmarkAsync
    {
        private List<byte[]> _allRequests;
        private Guid _playerId;
        private int _requestCount;
        private int cycles;
        private string _url;

        public int RequestCount
        {
            get { return _requestCount; }
        }

        private ManualResetEvent _doneEvent;

        public BenchmarkAsync(string url, int cycles, List<byte[]> allRequests, ManualResetEvent doneEvent)
        {
            this._url = url;
            this.cycles = cycles;
            _doneEvent = doneEvent;
            _allRequests = allRequests;
        }

        public void Start(Object threadContext)
        {
            for (var i = 0; i < cycles; i++)
            {
                DoCycle().Wait();
            }

            _doneEvent.Set();
        }

        private async Task DoCycle()
        {
            var initRequest = new Request
            {
                InitRequest = new InitRequest(),
                DeviceUniqueId = Guid.NewGuid().ToString(),
                UserId = Guid.Empty.ToByteArray()
            };

            var responseContainer = await SendRequest(initRequest);
            var response = MessagePackSerializer.Deserialize<Response>(responseContainer.Response);

            //Console.WriteLine(response.ResponseCode + " " + response.ServerStackTrace + " " + response.PlayerId);

            _playerId = new Guid(response.PlayerId);

            await SendSavedRequests();
        }

        private async Task SendSavedRequests()
        {
            for (var i = 0; i < _allRequests.Count; i++)
            {
                var requestBytes = _allRequests[i];
                var requestDataContainer =
                    MessagePackSerializer.Deserialize<DataCollection>(MessageCoder.GetData(requestBytes));
                var header = MessagePackSerializer.Deserialize<Request>(requestDataContainer.Request);

                header.UserId = _playerId.ToByteArray();

                var dataCollectionUpdated = new DataCollection
                {
                    Data = requestDataContainer.Data,
                    State = requestDataContainer.State,
                    Request = MessagePackSerializer.Serialize(header)
                };

                var postDataUnsigned = MessagePackSerializer.Serialize(dataCollectionUpdated);
                var postDataSigned = MessageCoder.AddSignature(postDataUnsigned);

                bool lastRequest = (i == (_allRequests.Count - 1));

                Interlocked.Increment(ref _requestCount);

                await SendRequest(postDataSigned).ContinueWith((t) =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine(t.Exception);
                        return;
                    }

                    var responseContainer = t.Result;
                    var response = MessagePackSerializer.Deserialize<Response>(responseContainer.Response);

                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Console.WriteLine(response.ResponseCode + " " + response.ServerStackTrace);
                    }
                });
            }
        }

        private async Task<DataCollection> SendRequest(Request initRequest)
        {
            var requestContainer = new DataCollection()
            {
                Request = MessagePackSerializer.Serialize(initRequest)
            };

            var serialized = MessagePackSerializer.Serialize(requestContainer);
            var serializedSigned = MessageCoder.AddSignature(serialized);

            return await SendRequest(serializedSigned);
        }

        private async Task<DataCollection> SendRequest(byte[] serializedSigned)
        {
            using (var client = new WebClient())
            {
                var responseBytes = await client.UploadDataTaskAsync(_url, serializedSigned);
                return MessagePackSerializer.Deserialize<DataCollection>(responseBytes);
            }
        }
    }
}
