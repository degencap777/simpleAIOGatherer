using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ennui.Script.Official
{
    public class StateMonitor<T>
    {
        private readonly int iterationCount;
        private readonly T expected;

        private readonly List<T> timestamps = new List<T>();

        public StateMonitor(int iterationCount, T expected)
        {
            this.iterationCount = iterationCount;
            this.expected = expected;
        }

        public bool Stamp(T value)
        {
            timestamps.Add(value);
            while (timestamps.Count >= iterationCount)
            {
                timestamps.RemoveAt(0);
            }

            foreach (var stamp in timestamps)
            {
                if (!stamp.Equals(expected))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
