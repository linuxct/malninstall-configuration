using System;
using System.Threading;

namespace space.linuxct.malninstall.Configuration.Helpers.Random
{
    /// <summary>
    /// Random provider thanks to https://csharpindepth.com/Articles/Random
    /// </summary>
    public static class RandomProvider
    {    
        private static int _seed = Environment.TickCount;
        private static readonly ThreadLocal<System.Random> RandomWrapper = new(() => new System.Random(Interlocked.Increment(ref _seed)));

        public static System.Random GetThreadRandom()
        {
            return RandomWrapper.Value;
        }
    }
}