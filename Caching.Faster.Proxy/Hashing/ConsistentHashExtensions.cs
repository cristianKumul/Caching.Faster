namespace Caching.Faster.Proxy.Hashing
{
    public static class ConsistentHashExtensions
    {
        public static unsafe long GetConsistentHashCode(this string str)
        {
            unsafe
            {
                fixed (char* src = str)
                {
                    long hash1 = 53815660839411;
                    long hash2 = hash1;

                    int c;
                    char* s = src;
                    while ((c = s[0]) != 0)
                    {
                        hash1 = ((hash1 << 5) + hash1) ^ c;
                        c = s[1];
                        if (c == 0)
                            break;
                        hash2 = ((hash2 << 5) + hash2) ^ c;
                        s += 2;
                    }

                    return hash1 + (hash2 * 1566083941);
                }
            }
        }
    }
}
