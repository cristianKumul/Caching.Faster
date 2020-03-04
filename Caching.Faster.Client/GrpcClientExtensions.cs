﻿using Caching.Faster;
using Caching.Faster.Common;
using Caching.Faster.Proxy.Client.Options;
using Google.Protobuf;
using Grpc.Core;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Caching.Faster.Proxy.Client
{
    public static class GrpcClientExtensions
    {
        public static IServiceCollection AddProxyClient(this IServiceCollection services, IOptions<GrpcClientOptions> options)
        {
            var channel = new Channel(
                    options.Value.Host, options.Value.Port,
                    ChannelCredentials.Insecure,
                    new List<ChannelOption> {
                                new ChannelOption(ChannelOptions.MaxReceiveMessageLength,  6 * 1024 * 1024 ) });

            services.AddSingleton(channel);
            services.AddScoped(_ =>
            {
                var grpcChannel = _.GetService<Channel>();
                return new ProxyGrpcClient(grpcChannel);
            });
            return services;
        }

        public static GetRequest GetRequest(this IEnumerable<string> keys)
        {
            var request = new GetRequest();

            request.Key.AddRange(keys);
            
            return request;
        }

        public static SetRequest SetRequest<T>(this IEnumerable<Models.KeyValuePair<T>> keys)
        {
            var request = new SetRequest();

            request.Pairs.AddRange(keys.Select(key => new KeyValuePair() { 
                Key = key.Key, 
                Value = ByteString.CopyFrom(MessagePackSerializer.Serialize(key.Value).AsSpan()), 
                Ttl = key.Ttl 
            }));

            return request;
        }

        public static SetRequest SetRequest(this IEnumerable<string> keys)
        {
            var request = new SetRequest();

            request.Pairs.AddRange(keys.Select(key => new KeyValuePair() { Key = key }));

            return request;
        }

        public static IEnumerable<KeyValuePair> GetKeyValuePair(this GetResponse response)
        {
            if (response.Results.Count > 0)
            {
                foreach (var pair in response.Results)
                {
                    yield return pair;
                }
            }
        }
    }
}
