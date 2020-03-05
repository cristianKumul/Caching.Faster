using Caching.Faster.Proxy;
using Caching.Faster.Proxy.Client;
using Caching.Faster.Proxy.Client.Options;
using Caching.Faster.Worker;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Caching.Faster.TestConsole
{
    class Program
    {
        static async Task Main1(string[] args)
        {
            ThreadPool.SetMinThreads(500, 500);
            //await Task.Delay(35000);

            var services = new ServiceCollection();
            var grpcOptions = new GrpcClientOptions()
            {
                Host = "172.25.189.16",
                Port = 90
            };

            services.AddProxyClient(Options.Create(grpcOptions));

            var scope = services.BuildServiceProvider().CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<ProxyGrpcClient>();

            var result = await client.SetKey("mykey", new { hola = "hola" }, 1000);

            Console.WriteLine("Status Set keys {0}", result.Status);

            var getResult = await client.GetKey<object>("mykey");

            Console.WriteLine("Value of Get keys {0}", getResult);

            var delete = await client.DeleteKey("mykey");

            Console.WriteLine("Deleted keys {0}", delete.Status);

            getResult = await client.GetKey<object>("mykey");

            Console.WriteLine("Second validation after delete Get keys {0}", getResult);

            var deleteNonExists = await client.DeleteKey("mykey-notexists");

            Console.WriteLine("Deleted keys {0}", deleteNonExists?.Status ?? false);
        }

        static async Task Main2(string[] args)
        {
            await Task.Delay(35000);
            ThreadPool.SetMinThreads(25000, 25000);
          
            var channel0 = new Channel("localhost", 91, ChannelCredentials.Insecure);
            var channel1 = new Channel("localhost", 91, ChannelCredentials.Insecure);
            var channel2 = new Channel("localhost", 91, ChannelCredentials.Insecure);
            var client0 = new ProxyCache.ProxyCacheClient(channel0);
            var client1 = new ProxyCache.ProxyCacheClient(channel1);
            var client2 = new ProxyCache.ProxyCacheClient(channel2);
            var response = client0.Set(SetPRequest("superkey", "hola mundo"));

            Console.WriteLine("Status Set keys {0}", response.Results.FirstOrDefault().Status);

            var valor = client0.Get(GetPRequest("superkey"));

            Console.WriteLine("Fetch Value  {0}", valor.Results.FirstOrDefault().Value.ToStringUtf8());

            var deleted = await client0.DeleteAsync(SetPRequest("superkey"));

            Console.WriteLine("Value deleted {0}", deleted.Results.FirstOrDefault().Status);

            var deletedNonExist = await client0.DeleteAsync(SetPRequest("superkey-notexists"));

            Console.WriteLine("Value not deleted {0}", deletedNonExist?.Results?.FirstOrDefault()?.Status);

            var sw = new Stopwatch();
            for (int i = 0; i < 1000000; i++)
            {


                sw.Restart();
                //for (int ix = 0; ix < 10000; ix++)
                //{
                //   await client0.GetAsync(GetRequest("superkey"));
                //    // await client0.SetAsync(SetRequest("superkey", "hola mundo"));
                //}

                var tasks = new[]
                {
                    run(client0),
                    run(client1),
                    run(client2),
                    run(client0),
                    run(client1),
                    run(client2),
                    run(client0),
                    run(client1),
                    run(client2),
                    run(client0),
                    run(client1),
                    run(client2),
                    run(client0),
                    run(client1),
                    run(client2),
                    run(client0),
                    run(client1),
                    run(client2),
                    run(client0),
                    run(client1),
                    run(client2),
                    run(client0),
                    run(client1),
                    run(client2),
                    run(client0),
                    run(client1),
                    run(client2),
                    run(client0),
                    run(client1),
                    run(client2),
                    run(client0),
                    run(client1),
                    run(client2),
                    run(client0),
                    run(client1),
                    run(client2),
                    run(client0),
                    run(client1)
                };
                await Task.WhenAll(tasks);
                Console.WriteLine($"Elapsed {sw.ElapsedMilliseconds} ratio {40000 / sw.Elapsed.TotalSeconds }");
                await Task.Delay(4000);


            }
        }
        static async Task Main(string[] args)
        {
            ThreadPool.SetMinThreads(500, 500);
            await Task.Delay(25000);

            //var httpClientHandler = new HttpClientHandler();
            //httpClientHandler.CheckCertificateRevocationList = false;
            ////httpClientHandler.PreAuthenticate = false;
            //httpClientHandler.SslProtocols = System.Security.Authentication.SslProtocols.Default;
            //httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
            //// Return `true` to allow certificates that are untrusted/invalid
            //httpClientHandler.ServerCertificateCustomValidationCallback =
            //    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            //var httpClient = new HttpClient(httpClientHandler);

            //var channel0 = GrpcChannel.ForAddress("https://172.25.182.182:443",
            //    new GrpcChannelOptions { HttpClient = httpClient });
            var channel0 = new Channel("localhost", 90, ChannelCredentials.Insecure);
            //var channel0 =  GrpcChannel.ForAddress("localhost:90");
            //var channel3 = GrpcChannel.ForAddress("https://172.25.164.211:80");
            //var channel1 = GrpcChannel.ForAddress("https://172.25.164.211:80");
            //var channel2 = GrpcChannel.ForAddress("https://172.25.164.211:80");
            var client0 = new GrpcWorker.GrpcWorkerClient(channel0);
            //var client1 = new GrpcWorker.GrpcWorkerClient(channel0);
            //var client2 = new GrpcWorker.GrpcWorkerClient(channel0);
            //var client3 = new GrpcWorker.GrpcWorkerClient(channel0);
            // await run(client0);
            var sw = new Stopwatch();

         //   var valor2 = client0.Get(GetRequest("superkey"));


            
            var response = client0.Set(SetRequest("superkey", "hola mundo"));
            var response2 = client0.Set(SetRequest("superkey1", "hola mundo"));
            var response3 = client0.Set(SetRequest("superkey2", "hola mundo"));
            var valor = client0.Get(GetRequest("superkey"));
            Console.WriteLine($"value is {valor.Results[0].Value?.Length}");
            await Task.Delay(15000);
             valor = client0.Get(GetRequest("superkey"));
            Console.WriteLine($"now is value is {valor.Results[0].Value?.Length}");

            var valueDeleted = client0.Delete(SetRequest("superkey", ""));
            Console.WriteLine($"Values has been deleted {valueDeleted.Results[0].Status}");

            var valueNotDeleted = client0.Delete(SetRequest("superkey-notexist", ""));
            Console.WriteLine($"Values has been deleted {valueNotDeleted.Results.FirstOrDefault()?.Status ?? false}");

            valor = client0.Get(GetRequest("superkey"));
            Console.WriteLine($"now is value is {valor.Results[0].Value?.Length}");
            // Console.ReadLine();
            //var s = valor.Results[0].Value.ToString(UTF8Encoding.UTF8);

            for (int i = 0; i < 1000000; i++)
            {


                sw.Restart();
                //for (int ix = 0; ix < 10000; ix++)
                //{
                //   await client0.GetAsync(GetRequest("superkey"));
                //    // await client0.SetAsync(SetRequest("superkey", "hola mundo"));
                //}

                var tasks = new[]
                {
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0),
                    run(client0)
                };
                await Task.WhenAll(tasks);
                Console.WriteLine($"Elapsed {sw.ElapsedMilliseconds} ratio {40000 / sw.Elapsed.TotalSeconds }");
                await Task.Delay(4000);


            }

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

        private static Task run(ProxyCache.ProxyCacheClient client)
        {
            for (int i = 0; i < 1000; i++)
            {
                client.SetAsync(SetPRequest(Guid.NewGuid().ToString(), "hola mundo"));
            }
            return Task.CompletedTask;
        }

        private static Task run(GrpcWorker.GrpcWorkerClient client)
        {
            for (int i = 0; i < 1000; i++)
            {
                client.SetAsync(SetRequest(Guid.NewGuid().ToString(), "hola mundo"));
            }
            return Task.CompletedTask;
        }

        //private static Task run2(Caching.Faster.Cache.CacheClient client)
        //{
        //    Console.WriteLine("Starting");
        //    int success = 0;
        //    int notok = 0;
        //    var k = new List<string>();
        //    var sw = new Stopwatch();
        //    sw.Start();
        //    for (int i = 0; i < 100; i++)
        //    {
        //        var g = Guid.NewGuid().ToString();

        //        k.Add(g);
        //        client.Set(SetRequest(g, "hola mundo"));
        //        if (i % 1000 == 0)
        //        {
        //            Console.WriteLine($"{sw.ElapsedMilliseconds} about {sw.ElapsedMilliseconds / 1000}ms each");
        //            sw.Restart();
        //        }
        //    }
        //    for (int i = 0; i < 100; i++)
        //    {
        //        var r = client.Get(GetRequest(k[i]));
        //        if (r.Results[0].Value != null)
        //        {
        //            success++;
        //        }
        //        else
        //        {
        //            notok++;
        //        }
        //        if (i % 1000 == 0)
        //        {
        //            Console.WriteLine($"{sw.ElapsedMilliseconds} about {sw.ElapsedMilliseconds / 1000}ms each");
        //            sw.Restart();
        //        }
        //    }
        //    Console.WriteLine($"Ok {success}:{notok} ");
        //    return Task.CompletedTask;
        //}
        private static SetWorkerRequest SetRequest(string key, string value)
        {
            var rq = new SetWorkerRequest();
            var pair = new Common.KeyValuePair
            {
                Key = key,
                Ttl = Convert.ToInt32(TimeSpan.Parse("00:01:00").TotalSeconds),
                Value = ByteString.CopyFrom(Encoding.UTF8.GetBytes(value))
            };
            rq.Pairs.Add(pair);

            return rq;

        }

        private static GetWorkerRequest GetRequest(string key)
        {
            var rq = new GetWorkerRequest();
            rq.Key.Add(key);

            return rq;

        }

        private static SetRequest SetPRequest(string key)
        {
            var rq = new SetRequest();
            var pair = new Common.KeyValuePair
            {
                Key = key,
                Value = ByteString.Empty
            };
            rq.Pairs.Add(pair);

            return rq;

        }

        private static Caching.Faster.Proxy.SetRequest SetPRequest(string key, string value)
        {
            var rq = new SetRequest();
            var pair = new Common.KeyValuePair
            {
                Key = key,
                Ttl = 15,
                Value = ByteString.CopyFrom(Encoding.UTF8.GetBytes(value))
            };
            rq.Pairs.Add(pair);

            return rq;

        }

        private static GetRequest GetPRequest(string key)
        {
            var rq = new GetRequest();
            rq.Key.Add(key);

            return rq;

        }
    }
}
