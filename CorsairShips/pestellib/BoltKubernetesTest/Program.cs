using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
//using k8s;
using Newtonsoft.Json;

namespace BoltKubernetesTest
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        
        static async Task Main(string[] args)
        {
//            var failedAllocationResponse = @"
//{
//  ""kind"": ""GameServerAllocation"",
//  ""apiVersion"": ""allocation.agones.dev/v1"",
//  ""metadata"": {
//    ""namespace"": ""default"",
//    ""creationTimestamp"": ""2020-01-31T11:43:06Z""
//  },
//  ""spec"": {
//    ""multiClusterSetting"": {
//      ""policySelector"": {}
//    },
//    ""required"": {
//      ""matchLabels"": {
//        ""agones.dev/fleet"": ""fleet-example""
//      }
//    },
//    ""scheduling"": ""Packed"",
//    ""metadata"": {}
//  },
//  ""status"": {
//    ""state"": ""UnAllocated"",
//    ""gameServerName"": """"
//  }
//}
//";


//            var allocationResponse = @"
//{
//	""kind"": ""GameServerAllocation"",
//	""apiVersion"": ""allocation.agones.dev/v1"",
//	""metadata"": {
//		""name"": ""fleet-example-mdgfz-rcffn"",
//		""namespace"": ""default"",
//		""creationTimestamp"": ""2020-01-31T11:06:16Z""
//	},
//	""spec"": {
//		""multiClusterSetting"": {
//			""policySelector"": {}
//		},
//		""required"": {
//			""matchLabels"": {
//				""agones.dev/fleet"": ""fleet-example""
//			}
//		},
//		""scheduling"": ""Packed"",
//		""metadata"": {}
//	},
//	""status"": {
//		""state"": ""Allocated"",
//		""gameServerName"": ""fleet-example-mdgfz-rcffn"",
//		""ports"": [{
//				""name"": ""default"",
//				""port"": 7061
//			}
//		],
//		""address"": ""192.168.88.110"",
//		""nodeName"": ""linuxserver1""
//	}
//}";

//            dynamic results = JsonConvert.DeserializeObject<dynamic>(failedAllocationResponse);
//            var state = results.status.state;
//            if (state == "Allocated")
//            {
//                var port = results.status.ports[0].port;
//                var address = results.status.address;
//                Console.WriteLine($"You can join to {address}:{port}");
//            } 
//            else
//            {
//                Console.WriteLine("Wrong response: " + state);
//            }

            //var response = JsonConvert.DeserializeObject(allocationResponse);

            await Task.Delay(3000); //waiting for sidecar
            var allocationResult = await AllocateGameServer();
            if (allocationResult != null)
            {
                Console.WriteLine($"Allocated server: {allocationResult.Value.address}:{allocationResult.Value.port}");
            }
        }

        private static async Task<(string address, int port)?> AllocateGameServer(string fleet = "fleet-example")
        {
            var requestTemplate = @"
{
    ""apiVersion"":""allocation.agones.dev/v1"",
    ""kind"":""GameServerAllocation"",
    ""spec"":{
        ""required"":{
            ""matchLabels"":{
                ""agones.dev/fleet"":""%FLEET%""
            }
        }
    }
}";

            var requestString = requestTemplate.Replace("%FLEET%", fleet);

            try
            {
                var content = new StringContent(requestString, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var resp = await client.PostAsync("http://localhost:8001/apis/allocation.agones.dev/v1/namespaces/default/gameserverallocations", content);
                if (resp.IsSuccessStatusCode)
                {
                    var responseText = await resp.Content.ReadAsStringAsync();

                    dynamic results = JsonConvert.DeserializeObject<dynamic>(responseText);
                    var state = results.status.state;
                    if (state == "Allocated")
                    {
                        var port = results.status.ports[0].port;
                        var address = results.status.address;
                        Console.WriteLine($"You can join to {address}:{port}");
                        return (address, port);
                    }
                    else
                    {
                        Console.WriteLine("Wrong response state: " + state);
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"Bad HTTP response code: {resp}");
                }
            } 
            catch (Exception e)
            {
                Console.WriteLine("Unepected exception during game server allocation: " + e.Message + "\n" + e.StackTrace);
                return null;
            }

            return null;
        }
    }
}
