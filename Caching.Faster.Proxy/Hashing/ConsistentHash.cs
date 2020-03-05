using Caching.Faster.Abstractions;
using Caching.Faster.Proxy.ServiceDiscovery.GKE.HostedServices;
using Caching.Faster.Workers.Client;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Caching.Faster.Worker.GrpcWorker;

namespace Caching.Faster.Proxy.Hashing
{
    public class ConsistentHash
    {
        bool _initialized = false;
        int _replicate = 300;    //default _replicate count
        int[] _orderedKeys = null;    //cache the ordered keys for better performance

        readonly SortedDictionary<int, Caching.Faster.Abstractions.Worker> circle = new SortedDictionary<int, Caching.Faster.Abstractions.Worker>();
        readonly SortedDictionary<string, GrpcWorkerClient> channels = new SortedDictionary<string, GrpcWorkerClient>();

        public ILogger<ConsistentHash> Logger { get; }

        private readonly static Gauge totalWorkers = Metrics.CreateGauge("faster_total_workers", "Number of workers connected to the proxy");

        public ConsistentHash(ILogger<ConsistentHash> logger)
        {
            K8SServiceDiscoveryHostedService.OnDiscoveryCompleted += K8SServiceDiscoveryHostedService_OnDiscoveryCompleted;
            Logger = logger;
        }

        private void K8SServiceDiscoveryHostedService_OnDiscoveryCompleted(object sender, FasterWorkers e)
        {
            var workers = e.GetWorkers().Where(node => node.Port > 0 && !string.IsNullOrEmpty(node.Address) && !string.IsNullOrWhiteSpace(node.Address));
            totalWorkers.Set(workers.Count());

            if (!_initialized)
            {
                Init(workers);
            }
            else
            {

                foreach (var w in workers)
                {
                    var found = false;
                    foreach (var cw in circle.Values)
                    {

                        if (cw.Address == w.Address)
                        {
                            // we found it so lets update if necessary
                            if (!w.IsActive)
                            {
                                Remove(cw);
                            }

                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Add(w);
                    }
                }
            }
        }


        //it's better you override the GetHashCode() of T.
        //we will use GetHashCode() to identify different node.
        public void Init(IEnumerable<Caching.Faster.Abstractions.Worker> nodes)
        {
            Init(nodes, _replicate);
        }

        public void Init(IEnumerable<Caching.Faster.Abstractions.Worker> nodes, int replicate)
        {
            _initialized = true;

            _replicate = replicate;

            foreach (var node in nodes)
            {
                this.Add(node, true);
            }
            _orderedKeys = circle.Keys.ToArray();
        }

        public void Add(Caching.Faster.Abstractions.Worker node)
        {
            Add(node, true);
        }

        private void Add(Caching.Faster.Abstractions.Worker node, bool updateKeyArray)
        {
            Logger.LogInformation($"Joining {node.Name} with endpoint {node.Address} on port {node.Port}");

            if (!channels.TryGetValue(node.Address, out _))
            {
                channels.Add(node.Address, new GrpcWorkerClient(new Channel(node.Address, node.Port, ChannelCredentials.Insecure)));

                Logger.LogInformation($"Worker {node.Name} joined.");
            }

            for (var i = 0; i < _replicate; i++)
            {
                var h = $"{node.Name}{node.Port}{node.Address}{i}".GetConsistentHashCode();
                var hash = BetterHash($"{h}");
                circle[hash] = node;
            }

            if (updateKeyArray)
            {
                _orderedKeys = circle.Keys.ToArray();
            }
        }

        public void Remove(Caching.Faster.Abstractions.Worker node)
        {
            Logger.LogInformation($"Joining {node.Name} with endpoint {node.Address} on port {node.Port}");

            if (channels.TryGetValue(node.Address, out _))
            {
                channels.Remove(node.Address);

                Logger.LogInformation($"Worker {node.Name} removed.");
            }

            for (var i = 0; i < _replicate; i++)
            {
                var h = $"{node.Name}{node.Port}{node.Address}{i}".GetConsistentHashCode();

                var hash = BetterHash($"{h}");

                if (!circle.Remove(hash))
                {
                    throw new Exception("cannot remove a node that not added");
                }
            }
            _orderedKeys = circle.Keys.ToArray();
        }

        //return the index of first item that >= val.
        //if not exist, return 0;
        //ay should be ordered array.
        int First_ge(int[] ay, int val)
        {
            var begin = 0;
            var end = ay.Length - 1;

            if (ay[end] < val || ay[0] > val)
            {
                return 0;
            }

            while (end - begin > 1)
            {
                var mid = (end + begin) / 2;
                if (ay[mid] >= val)
                {
                    end = mid;
                }
                else
                {
                    begin = mid;
                }
            }

            if (ay[begin] > val || ay[end] < val)
            {
                throw new Exception("should not happen");
            }

            return end;
        }

        public Caching.Faster.Abstractions.Worker GetNode(string key)
        {
            //return GetNode_slow(key);

            var hash = BetterHash(key);

            var first = First_ge(_orderedKeys, hash);

            //int diff = circle.Keys[first] - hash;

            return circle[_orderedKeys[first]];
        }

        // need to move this to another class
        public GrpcWorkerClient GetGrpcChannel(Caching.Faster.Abstractions.Worker key)
        {
            return channels[key.Address];
        }

        //default String.GetHashCode() can't well spread strings like "1", "2", "3"
        public static int BetterHash(string key)
        {
            var hash = MurmurHash2.Hash(Encoding.ASCII.GetBytes(key));
            return (int)hash;
        }
    }
}
