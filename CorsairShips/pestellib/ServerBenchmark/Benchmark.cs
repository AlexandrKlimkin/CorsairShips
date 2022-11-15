using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using MessagePack;
using S;
using PestelLib.ServerShared;

namespace ServerBenchmark
{
    public class Benchmark
    {
        private List<byte[]> _allRequests;
        private Guid _playerId;
        private int _requestCount;
        private int cycles;
        private string _url;
        private WebClient client = new WebClient();

        public int RequestCount
        {
            get { return _requestCount; }
        }

        private ManualResetEvent _doneEvent;

        public Benchmark(string url, int cycles, List<byte[]> allRequests, ManualResetEvent doneEvent)
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
                DoCycle();
            }

            _doneEvent.Set();
        }

        private void DoCycle()
        {
            var initRequest = new Request
            {
                InitRequest = new InitRequest(),
                DeviceUniqueId = Guid.NewGuid().ToString(),
                UserId = Guid.Empty.ToByteArray()
            };

            ResponseCode respCode = ResponseCode.UNDEFINED_RESPONSE;

            for (var i = 0; i < 100 && respCode != ResponseCode.OK; i++)
            {
                var responseContainer = SendRequest(initRequest);
                var response = MessagePackSerializer.Deserialize<Response>(responseContainer.Response);
                respCode = response.ResponseCode;

                //Console.WriteLine(response.ResponseCode + " " + response.ServerStackTrace + " " + response.PlayerId);
                if (respCode == ResponseCode.OK)
                {
                    _playerId = new Guid(response.PlayerId);
                    break;
                }
            }

            SendSavedRequests();
        }

        private void SendSavedRequests()
        {
            for (var i = 0; i < _allRequests.Count; i++)
            {
                var requestBytes = _allRequests[i];
                var requestDataContainer = MessagePackSerializer.Deserialize<DataCollection>(MessageCoder.GetData(requestBytes));
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

                _requestCount++;

                for (var j = 0; j < 100; j++)
                {
                    var responseContainer = SendRequest(postDataSigned);
                    var response = MessagePackSerializer.Deserialize<Response>(responseContainer.Response);

                    if (response.ResponseCode == ResponseCode.OK)
                    {
                        break;
                    }
                    else
                    {
                        string serverMessage = new StringReader(response.ServerStackTrace).ReadLine();
                        Console.WriteLine(response.ResponseCode + " " + serverMessage);
                    }
                }
            }
        }



        private DataCollection SendRequest(Request initRequest)
        {
            var requestContainer = new DataCollection()
            {
                Request = MessagePackSerializer.Serialize(initRequest)
            };

            var serialized = MessagePackSerializer.Serialize(requestContainer);
            var serializedSigned = MessageCoder.AddSignature(serialized);

            return SendRequest(serializedSigned);
        }

        private DataCollection SendRequest(byte[] serializedSigned)
        {
            var responseBytes = client.UploadData(_url, serializedSigned);
            return MessagePackSerializer.Deserialize<DataCollection>(responseBytes);
        }
    }
}
