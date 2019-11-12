using Caching.Faster.Abstractions;
using k8s;
using k8s.Models;
using System;
using System.Linq;

namespace Caching.Faster.Proxy.ServiceDiscovery.GKE
{
    public class K8SServiceDiscovery
    {
        private readonly KubernetesClientConfiguration configuration;
        private readonly Kubernetes kubernetesClient;

        // this is intended for caching arrays
        private const int resizeFactor = 32;
        
        // cached namespaces that refresh every 5 minutes
        private string[] namespaces;

        // cached namespaces containing workers that refresh every 15 secs
        private string[] observedNamespaces;

        // cached workers that refresh every 15 secs
        private FasterWorkers workers = new FasterWorkers();

        // event to notify for changes
        public event EventHandler<FasterWorkers> OnDiscoveryCompleted;

        public K8SServiceDiscovery()
        {
            // custom implementation of InclusterConfig
            configuration = KubernetesDiscoveryClientConfiguration.InClusterConfig();

            // kubernetes client
            kubernetesClient = new Kubernetes(configuration);

            // slots for cached namespaces
            namespaces = new string[250];

            // slots forcached observed namespaces
            observedNamespaces = new string[250];

            // lets try the first discover attempt
            Initilize();
        }

        public void Initilize()
        {
            DiscoverNamespaces();
            DiscoverWorkers();
        }

        /// <summary>
        /// this method should be called in 15s period
        /// </summary>
        public void RefreshWorkers()
        {
            //will go through all observed namespaces to figureout how many workers we have
            foreach (var ns in observedNamespaces)
            {
                if (ns is null)
                    continue;

                // so we have a pod list, lets parse it
                ParsePod(ns, kubernetesClient.ListNamespacedPod(ns), false);
            }

            // may be someone is listening so lets fire this event
            OnDiscoveryCompleted?.Invoke(this, workers);
        }


        /// <summary>
        /// this method is expensive should be called in 2m period
        /// </summary>
        public void DiscoverWorkers()
        {
            //will go through all namespaces to figureout how many workers we have
            foreach (var ns in namespaces)
            {
                if (ns is null)
                    continue;
                
                // so we have a pod list, lets parse it
                ParsePod(ns, kubernetesClient.ListNamespacedPod(ns), true);
            }

            // may be someone is listening so lets fire this event
            OnDiscoveryCompleted?.Invoke(this, workers);
        }

        private void ParsePod(string ns, V1PodList list, bool observe = false)
        {
            // lets try to find a worker
            foreach (var item in list.Items)
            {
                // it's weird but some pods does not have annotations so lets figure out
                if (item.Metadata?.Annotations?.Count > 0)
                {
                    // those labels indicates that this pod is actually a worker so we should consider to add it to the hashing ring
                    item.Metadata.Annotations.TryGetValue(Annotations.CachingFasterEnabled, out string isEnabled);
                    item.Metadata.Annotations.TryGetValue(Annotations.CachingFasterScrapePort, out string listenPort);

                    // we need both values to proceed
                    if (isEnabled is null || listenPort is null)
                        continue;

                    // lets keep an eye on this namespace
                    if (observe)
                        ObservedNamespaces(ns);

                    // may be the pod is market as disabled so lets validate it
                    if (isEnabled.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var status = default(V1ContainerStatus);

                        // lets check if ready
                        if ((status = item.Status?.ContainerStatuses?.FirstOrDefault()) != null)
                        {
                            // so add or refresh the worker with current status
                            workers.Join(item.Metadata?.Name, item.Status?.PodIP, Convert.ToInt32(listenPort), status.Ready);
                        }
                        else
                        {
                            // we could not retrieve status metadata, so lets mark as inactive
                            workers.SetStatus(item.Status?.PodIP, false, false);
                        }
                    }
                    else
                    {
                        // we could not retrieve status metadata, so lets mark as inactive the pod is marked as disabled so lets refresh our ring if neccessary
                        workers.SetStatus(item.Status?.PodIP, false, true);
                    }
                }
            }
        }

        /// <summary>
        /// This will discover all namespaces in the cluster
        /// </summary>
        /// <param name="ns"></param>
        private void ObservedNamespaces(string ns)
        {

            bool saved = false;

            //search for an empy slot to save                     
            for (int i = 0; i < observedNamespaces.Length; i++)
            {
                // if already exists just leave
                if (observedNamespaces[i] == ns)
                    break;


                if (observedNamespaces[i] is null)
                {
                    saved = true;
                    // we found a slot so just save and go home
                    observedNamespaces[i] = ns;
                    break;
                }
            }

            if (!saved)
            {
                // lets save actual size
                int size = observedNamespaces.Length;

                // ok we reach max and we could not save it, lets resize 
                Array.Resize(ref observedNamespaces, observedNamespaces.Length + resizeFactor);

                // so now we can save it
                observedNamespaces[size] = ns;
            }

        }

        /// <summary>
        /// this method is expensive should be called in 15m period
        /// </summary>
        public void DiscoverNamespaces()
        {
            var nsd = kubernetesClient.ListNamespace().Items;

            // if there are not space for new namespaces lets add more slots
            if (namespaces.Length < nsd.Count)
            {
                Array.Resize(ref namespaces, nsd.Count + resizeFactor);
            }
                

            // setting new ones
            for (int i = 0; i < nsd.Count; i++)
            {
                namespaces[i] = nsd[i].Metadata.Name;
            }

            // it's possible to have less namespaces this time, so lets clear the rest
            for (int i = nsd.Count; i < namespaces.Length; i++)
            {
                namespaces[i] = null;
            }

        }

    }
}
