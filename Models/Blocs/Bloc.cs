using MineIntoTheDeep.Models.Pioches;

namespace MineIntoTheDeep.Models.Blocs
{
    public class Bloc(Ore? ore, int x, int y, int quantity, int life = 3)
    {
        // Instance variables
        public Guid Id { get; init; } = Guid.NewGuid();
        public Ore? Ore { get; init; } = ore;
        public int X { get; init; } = x;
        public int Y { get; init; } = y;
        public int Quantity { get; init; } = quantity;
        public int Life { get; set; } = life;

        //
        // Fonctions
        // 

        /// <summary>
        /// Mines the bloc meaning that the life of the bloc reduces of the damages of the pickaxe. If 
        /// its life is bellow zeto then it breaks and returns the reward otherwise no rewards.
        /// </summary>
        /// <param name="pioche"> The pickaxe used to mine the bloc </param>
        /// <exception cref="InvalidOperationException"> If the bloc is mined bu already broken </exception>
        /// <returns> The reward if the bloc broke meaning the ore's value * the quantity or 0 else </returns>
        public int Mine(Pioche pioche)
        {
            if (Life <= 0)
            {
                throw new InvalidOperationException("The life bloc was already broken but mined again !");
            }
            Life -= pioche.Damages;
            return GetTotalValue() * ((Life <= 0) ? 1 : 0);
        }

        /// <summary>
        /// Gets the total value of the bloc meaning value * quantity
        /// </summary>
        /// <returns> The value of the bloc </returns>
        public int GetTotalValue() {
            return ((Ore != null) ? Ore.Value : 0) * Quantity;
        }

        /// <summary>
        /// Tests if the bloc is broken
        /// </summary>
        /// <returns> True if the bloc is broken meaning that life <= 0. False otherwise </returns>
        public bool Broken() {
            return Life <= 0;
        }

        // Overrides
        public override string ToString()
        {
            return "Bloc : O " + ((Ore != null) ? Ore.Name : "None") + $" X {X} Y {Y} Q {Quantity} L {Life}";
        }
    }
}