using Ennui.Api;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public class StateMonitor<T> : ApiResource
    {
        private readonly int iterationCount;
        private readonly T[] expected;

        private readonly List<T> timestamps = new List<T>();

        public StateMonitor(IApi api, int iterationCount, params T[] expected) : base(api)
        {
            this.iterationCount = iterationCount;
            this.expected = expected;
        }

        private bool IsExpected(T value)
        {
            foreach (var t in expected)
            {
                if (t.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Stamp(T value)
        {
            Logging.Log("Add: " + value);
            timestamps.Add(value);
            while (timestamps.Count >= iterationCount)
            {
                timestamps.RemoveAt(0);
            }
            Logging.Log("Timestamp count: " + timestamps.Count);

            foreach (var stamp in timestamps)
            {
                Logging.Log("Timestamp val: " + stamp);
                if (!IsExpected(stamp))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
