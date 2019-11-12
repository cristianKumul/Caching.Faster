using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Caching.Faster.Abstractions
{
    public class FasterWorkers
    {
        private bool initialized;
        /// <summary>
        /// Dictionary with worker, active and mark for deletion
        /// </summary>
        private ConcurrentDictionary<string, Worker> workers;

        /// <summary>
        /// get worker by Address 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Worker this[string name] { get { return workers[name]; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worker"></param>
        public void Join(Worker worker)
        {
            CheckInit();

            workers.AddOrUpdate(worker.Name, worker, (s, w) => {  return worker; });
        }

        public void Join(string name, string address, int port, bool active = true)
        {
            var w = new Worker()
            {
                Address = address,
                Name = name,
                Port = port,
                IsActive = active
            };

            Join(w);
        }

        private void CheckInit()
        {
            // first time
            if (!initialized)
            {
                // run this once
                initialized = true;
                // max workers capacity to 200
                workers = new ConcurrentDictionary<string, Worker>(4, 250);
            }
        }

        public void SetStatus(string name, bool active = true, bool markForDeletion = false)
        {
            if (initialized)
            {
                var w = workers[name];

                w.IsActive = active;
                w.IsMarkedForDeletion = markForDeletion;

                workers.AddOrUpdate(w.Address, w, (s, ws) => w);
            }
        }

        public void DeleteMarkedWorkers()
        {
            var keys = workers.Keys.ToArray();

            for (var i = 0; i < keys.Count(); i++)
            {
                if (workers[keys[i]].IsMarkedForDeletion)
                {
                    workers.TryRemove(keys[i], out _);
                }
            }
        }

        public IEnumerable<Worker> GetWorkers()
        {
            return workers.Values.ToArray();
        }
    }
}
