using System;
using System.Collections.Generic;
using MineIntoTheDeep.Helpers;
using System.Linq;

namespace MineIntoTheDeep.Models.Blocs.Usine
{
    public class BlocUsine(Strate strate)
    {
        // Instance variables
        public Strate CurrentStrate { get; set; } = strate;

        //
        //  Functions
        //

        public Bloc GenerateBloc(int x, int y)
        {
            // Calculating the cummulated rate of ores appearence and making sure
            // that the sum of the rates < 1 otherwise there is a mistake
            List<KeyValuePair<Ore, double>> cummulatedRates = [.. CurrentStrate.Rates.OrderBy(pair => pair.Value)];
            for (int i = 1; i < cummulatedRates.Count; i++)
            {
                KeyValuePair<Ore, double> e = cummulatedRates[i];
                cummulatedRates[i] = new(e.Key, cummulatedRates[i - 1].Value + e.Value);
            }
            if (cummulatedRates.Count > 0 && cummulatedRates[^1].Value > 1)
            {
                throw new ArgumentException("The sum of the rates must be inferior or equals to 1 !");
            }

            // Generating the bloc with the appropriate ore or none
            int quantity = 0;
            Ore? ore = null;
            
            if (CurrentStrate.OreRate >= RandomGenerator.GenerateRandomDouble(0, 1))
            {
                quantity = RandomGenerator.GenerateRandomInt(CurrentStrate.MinQuantity, CurrentStrate.MaxQuantity);
                double oldCummulatedRate = 0;
                double rand = RandomGenerator.GenerateRandomDouble(0, 1);
                foreach(KeyValuePair<Ore, double> e in cummulatedRates) {
                    if (oldCummulatedRate < rand && rand <= e.Value) {
                        ore = e.Key;
                    }
                    oldCummulatedRate = e.Value;
                }
            } 

            return new Bloc(ore, x, y, quantity);
        }
    }
}