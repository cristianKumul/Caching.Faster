using System;

namespace Caching.Faster.Abstractions
{
    public struct Worker : IEquatable<Worker>
    {
        public bool IsActive;
        public bool IsMarkedForDeletion;
        public string Name;
        public string Address;
        public int Port;

        public bool Equals(Worker other)
        {
            return this.Name == other.Name && this.Address == other.Address && this.Port == other.Port;
        }

        public static bool operator !=(Worker a, Worker b)
        {
            return !a.Equals(b); 
        }
        public static bool operator ==(Worker a, Worker b)
        {
            return a.Equals(b);
        }
    }
}
