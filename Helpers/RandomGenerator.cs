using System;

namespace MineIntoTheDeep.Helpers
{
    /// <summary>
    /// Simply memories the instance of Random to improve 
    /// randomness
    /// </summary>
    public static class RandomGenerator
    {
        // Class instance
        private static readonly Random random = new();

        //
        //  Functions
        //

        public static int GenerateRandomInt(int a, int b)
        {
            return random.Next(a, b);
        }

        public static double GenerateRandomDouble(int a, int b)
        {
            return a + random.NextDouble() * (b - a);
        }
    }
}