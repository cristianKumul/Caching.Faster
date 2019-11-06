using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Caching.Faster.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //ThreadPool.SetMinThreads(15300, 15300);
            await Task.Delay(25000);
            var channel0 = GrpcChannel.ForAddress("https://localhost:81");
            //var channel0 = GrpcChannel.ForAddress("https://localhost:5001");
            //var channel1 = GrpcChannel.ForAddress("https://localhost:5001");
            //var channel2 = GrpcChannel.ForAddress("https://localhost:5001");
            var client0 = new Caching.Faster.Cache.CacheClient(channel0);
            //var client1 = new Caching.Faster.Cache.CacheClient(channel1);
            //var client2 = new Caching.Faster.Cache.CacheClient(channel2);
            //var client3 = new Caching.Faster.Cache.CacheClient(channel3);

            //var sw = new Stopwatch();
            var response = client0.Set(SetRequest("superkey", "hola mundo"));
            var valor = client0.Get(GetRequest("superkey"));
            //var s = valor.Results[0].Value.ToString(UTF8Encoding.UTF8);

            ////for (int i = 0; i < 1000000; i++)
            ////{


            ////    sw.Restart();
            ////    var tasks = new[]
            ////    {
            ////        run(client0),
            ////        run(client1),
            ////        run(client2),
            ////        run(client3),
            ////        run(client0),
            ////        run(client1),
            ////        run(client2),
            ////        run(client3),
            ////        run(client0),
            ////        run(client1),
            ////    };
            ////    Console.WriteLine($"Elapsed {sw.ElapsedMilliseconds} ratio {10000 / sw.Elapsed.TotalSeconds }");
            ////    await Task.WhenAll(tasks);

            ////}

            //valor = client0.Get(GetRequest("superkey"));
            //sw.Restart();
            //for (int i = 0; i < 1000000; i++)
            //{
            //    valor = client0.Get(GetRequest("superkey"));
            //}
            //Console.WriteLine($"Elapsed {sw.ElapsedMilliseconds}");

            ////if (Encoding.UTF8.GetString(valor.Results[0].Value.ToByteArray()) == "hola mundo")
            ////    Console.WriteLine("funciono!");

            var c = new Caching.Faster.Proxy.ServiceDiscovery.GKE.K8SServiceDiscovery();
            c.Initilize();
            Console.WriteLine("Hello World!");
        }
        public static int ma = 0;
        private static Task run(Caching.Faster.Cache.CacheClient client)
        {
            for (int i = 0; i < 1000; i++)
            {
                client.Set(SetRequest("superkey", "hola mundo"));
            }
            return Task.CompletedTask;
        }
        private static SetRequest SetRequest(string key, string value)
        {
            var rq = new SetRequest();
            var pair = new Proxy.KeyValuePair();
            pair.Key = key;
            pair.Value = ByteString.CopyFrom(Encoding.UTF8.GetBytes(value));
            rq.Pairs.Add(pair);

            return rq;

        }

        private static GetRequest GetRequest(string key)
        {
            var rq = new GetRequest();
            rq.Key.Add(key);

            return rq;

        }
    }
}
