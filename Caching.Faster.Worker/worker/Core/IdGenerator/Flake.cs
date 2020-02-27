using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caching.Faster.Worker.Core.IdGenerator
{

    public static class Id64Generator 
    {
        #region Private Constant

        /// <summary>
        /// 1 January 1970. Used to calculate timestamp (in milliseconds)
        /// </summary>
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private const long Epoch = 1351606710465L;

        /// <summary>
        /// Number of bits allocated for a worker id in the generated identifier. 5 bits indicates values from 0 to 31
        /// </summary>
        private const int WorkerIdBits = 5;

        /// <summary>
        /// Datacenter identifier this worker belongs to. 5 bits indicates values from 0 to 31
        /// </summary>
        private const int DatacenterIdBits = 5;

        /// <summary>
        /// Generator identifier. 10 bits indicates values from 0 to 1023
        /// </summary>
        private const int GeneratorIdBits = 10;

        /// <summary>
        /// Maximum generator identifier
        /// </summary>
        private const long MaxGeneratorId = -1 ^ (-1L << GeneratorIdBits);

        /// <summary>
        /// Maximum worker identifier
        /// </summary>
        private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);

        /// <summary>
        /// Maximum datacenter identifier
        /// </summary>
        private const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);

        /// <summary>
        /// Number of bits allocated for sequence in the generated identifier
        /// </summary>
        private const int SequenceBits = 12;

        private const int WorkerIdShift = SequenceBits;

        private const int DatacenterIdShift = SequenceBits + WorkerIdBits;

        private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

        private const long SequenceMask = -1L ^ (-1L << SequenceBits);

        #endregion Private Constant

        #region Private Fields

        /// <summary>
        /// Object used as a monitor for threads synchronization.
        /// </summary>
        private static readonly object monitor = new object();

        /// <summary>
        /// The timestamp used to generate last id by the worker
        /// </summary>
        private static long lastTimestamp = -1L;

        private static long sequence = 0;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Indicates how many times the given generator had to wait 
        /// for next millisecond <see cref="TillNextMillis"/> since startup.
        /// </summary>
        public static int NextMillisecondWait { get; set; }

        #endregion Public Properties



        #region Public Properties

        /// <summary>
        /// The identifier of the worker
        /// </summary>
        public static long WorkerId { get; private set; } = 0;

        /// <summary>
        /// Identifier of datacenter the worker belongs to
        /// </summary>
        public static long DatacenterId { get; private set; } = 0;

        #endregion Public Properties

        #region Public Methods

        public static long GenerateId()
        {
            lock (monitor)
            {
                return NextId();
            }
        }

        public static IEnumerator<long> GetEnumerator()
        {
            while (true)
            {
                yield return GenerateId();
            }
        }

        // System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}


        #endregion Public Methods

        #region Private Properties

        private static long CurrentTime
        {
            get { return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds; }
        }

        #endregion Private Properties

        #region Public Methods
        #endregion Public Methods

        #region Private Static Methods
        #endregion Private Static Methods

        #region Private Methods

        private static long TillNextMillis(long lastTimestamp)
        {
            NextMillisecondWait++;

            var timestamp = CurrentTime;

            SpinWait.SpinUntil(() => (timestamp = CurrentTime) > lastTimestamp);

            return timestamp;
        }

        private static long NextId()
        {
            var timestamp = CurrentTime;

            if (timestamp < lastTimestamp)
            {
                throw new InvalidOperationException(string.Format("Clock moved backwards. Refusing to generate id for {0} milliseconds", (lastTimestamp - timestamp)));
            }

            if (lastTimestamp == timestamp)
            {
                sequence = (sequence + 1) & SequenceMask;
                if (sequence == 0)
                {
                    timestamp = TillNextMillis(lastTimestamp);
                }
            }
            else
            {
                sequence = 0;
            }

            lastTimestamp = timestamp;
            return ((timestamp - Epoch) << TimestampLeftShift) |
                (DatacenterId << DatacenterIdShift) |
                (WorkerId << WorkerIdShift) |
                sequence;
        }

        #endregion Private Methods
    }
}
