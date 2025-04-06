using System;
using System.Collections.Generic;

namespace MineIntoTheDeep.Models
{
    public class Ore
    {
        // Class variables
        public static List<Ore> Ores { get; } = [];

        // Instance variables
        public string Name { get; init; }
        public int Value { get; init; }
        public double[] RateByStrate { get; init; }

        // Constructors
        public Ore(string name, int value, double[] rateByStrate)
        {
            if (rateByStrate.Length > 5)
            {
                throw new ArgumentException("There are only 5 strates. Therefore an Ore can not have more than 5 rate by strate !");
            }

            Name = name;
            Value = value;
            RateByStrate = rateByStrate;

            Register();
        }

        //
        //  Functions
        //

        /// <summary>
        /// Register it self : add its type to the list of ores so that we can have the list of ores
        /// in order to generate ores.
        /// </summary>
        private void Register()
        {
            if (!Ores.Contains(this))
            {
                Ores.Add(this);
            }
        }

        // Overrides
        public override string ToString()
        {
            return $"Ore : Name {Name} Value {Value}";
        }
    }
}