using System.Text;

namespace MineIntoTheDeep.Models
{
    public class Strate(int num, double oreRate, int minQuantity, int maxQuantity)
    {
        // Instance variables
        public Guid Id { get; init; } = Guid.NewGuid();
        public int Num { get; init; } = num;
        public double OreRate { get; protected init; } = oreRate;
        public Dictionary<Ore, double> Rates { get; init; } = [];
        public int MinQuantity { get; init; } = minQuantity;
        public int MaxQuantity { get; init; } = maxQuantity;
        public int Layers { get; set; }

        // Overrides
        public override string ToString()
        {
            StringBuilder bld = new($"Strate : Num {Num} OreRate {OreRate} MinQuantity {MinQuantity} MaxQuantity {MaxQuantity} Layers {Layers}\n\t");

            // The rates
            bld.Append("Rates :");
            foreach (KeyValuePair<Ore, double> e in Rates) {
                bld.Append($"\n\t\t{e.Key} => Rate {e.Value}");
            }

            return bld.ToString();
        }
    }
}