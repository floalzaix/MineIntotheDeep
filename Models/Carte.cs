using System.Text;
using MineIntoTheDeep.Helpers;
using MineIntoTheDeep.Models.Blocs;
using MineIntoTheDeep.Models.Blocs.Usine;

namespace MineIntoTheDeep.Models
{
    public class Carte
    {
        //
        // Class variables
        //

        // Parameters 
        private static readonly double ORE_RATE = 0.4;
        private static readonly int MIN_ORE_QUANTITY = 1;
        private static readonly int MAX_ORE_QUANTITY = 10;

        // Ores definition
        private static readonly Ore FER = new("Fer", 10, [1, 0.7, 0.4, 0.1, 0]);
        private static readonly Ore OR = new("Or", 20, [0, 0.3, 0.4, 0.3, 0.3]);
        private static readonly Ore DIAMANT = new("Diamant", 40, [0, 0, 0.2, 0.4, 0.3]);
        private static readonly Ore MITHRIL = new("Mithril", 80, [0, 0, 0, 0.2, 0.4]);

        // 
        // Instance variables
        //

        // Properties
        public Guid GameId { get; set; }
        public int NumberOfPlayer { get; init; }
        public int Size { get; init; } // Like size x size (6 x 6 or 9 x 9)
        public int Depth { get; init; }

        // Map related variables 
        public Strate[] Strates { get; init; }
        public Stack<Bloc>[,] Tunnels { get; init; }
        public Bloc[,] TopLayer { get; set; }

        // Player related variables
        public List<Mineur> Mineurs { get; init; }

        // Events
        public event EventHandler<string>? OnEnd;

        // Constructors
        public Carte(Guid gameId, int numberOfPlayer)
        {
            if (numberOfPlayer > 9)
            {
                throw new ArgumentException("The number of players can not be > to 9 ! ");
            }
            
            GameId = gameId;
            NumberOfPlayer = numberOfPlayer;

            if (NumberOfPlayer > 6)
            {
                Size = 9;
            }
            else
            {
                Size = 6;
            }

            Depth = RandomGenerator.GenerateRandomInt(10, 20);

            TopLayer = new Bloc[Size, Size];

            // Map generation 
            Strates = GenerateStartes();
            Tunnels = GenerateTunnels();

            UpdateTopLayer();

            // Players
            Mineurs = [];
        }

        //
        //  Functions
        //

        /// <summary>
        /// Generates the tunnels which are the blocs under the blocs. It uses the 
        /// factory to do so and the strate to handle the ore rates.
        /// </summary>
        /// <returns> The tunnels initialized </returns>
        /// <exception cref="ArgumentException"> If the number of strate is not correct </exception>
        private Stack<Bloc>[,] GenerateTunnels()
        {
            Stack<Bloc>[,] tunnels = new Stack<Bloc>[Size, Size];

            int strate = 4;
            int layerInStrate = 0;
            BlocUsine usine = new(Strates[strate]);
            for (int i = 0; i < Depth; i++)
            {
                for (int x = 0; x < Size; x++)
                {
                    for (int y = 0; y < Size; y++)
                    {
                        if (tunnels[x, y] == null)
                        {
                            tunnels[x, y] = new Stack<Bloc>();
                        }
                        tunnels[x, y].Push(usine.GenerateBloc(x, y));
                    }
                }

                layerInStrate += 1;

                // Switching from strate if needed so
                if (layerInStrate >= Strates[strate].Layers)
                {
                    layerInStrate = 0;

                    if (strate < 0)
                    {
                        throw new ArgumentException("The number of layers in the strates is wrong !");
                    }

                    strate -= 1;

                    if (strate >= 0)
                    {
                        usine = new(Strates[strate]);
                    }
                }
            }

            return tunnels;
        }

        /// <summary>
        /// Generates the strates logic. Meant for the variable Strates to be set to
        /// the return value of this function
        /// </summary>
        /// <returns> The strates which are initialized </returns>
        private Strate[] GenerateStartes()
        {
            Strate[] strates = new Strate[5];

            // Defining the strates and ore's rate
            for (int i = 0; i < 5; i++)
            {
                strates[i] = new Strate(i + 1, ORE_RATE, MIN_ORE_QUANTITY, MAX_ORE_QUANTITY);

                foreach (Ore ore in Ore.Ores)
                {
                    strates[i].Rates[ore] = ore.RateByStrate[i];
                }
            }

            // Defining the layers
            int strate = 4;
            for (int i = 0; i < Depth; i++)
            {
                strates[strate].Layers += 1;
                strate = (strate > 0) ? strate - 1 : 4;
            }

            return strates;
        }

        public void UpdateTopLayer()
        {
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    if (TopLayer[x, y] == null || TopLayer[x, y].Broken())
                    {
                        if (Tunnels[x, y].Count == 0) {
                            Mineur? m = GetMineurThere(x, y);
                            if (m != null) {
                                m.Joueur.Score -= 1000;
                            }
                            OnEnd?.Invoke(this, "END");
                        } else {
                            TopLayer[x, y] = Tunnels[x, y].Pop();
                            
                            Mineur? m = GetMineurThere(x, y);
                            if (m != null) {
                                m.BlocUnder = TopLayer[x, y];
                            }
                        }
                    }
                }
            }
        }

        public bool MineurThere(int x, int y)
        {
            foreach (Bloc b in Mineurs.Select(m => m.BlocUnder))
            {
                if (b.X == x && b.Y == y)
                {
                    return true;
                }
            }
            return false;
        }

        public Mineur? GetMineurThere(int x, int y) {
            if (Mineurs != null) {
                foreach (Mineur m in Mineurs) {
                    if (m.BlocUnder.X == x && m.BlocUnder.Y == y)
                    {
                        return m;
                    }
                }
            }
            return null;
        }

        public void Miner() {
            foreach (Mineur m in Mineurs) {
                m.Mine();
            }
        }

        public Bloc GetBlocUnder(int x, int y)
        {
            return TopLayer[x, y];
        }

        //
        //  Overrides
        //

        public override string ToString()
        {
            StringBuilder bld = new($"Carte :\n\tNumberOfPlayer {NumberOfPlayer}\n\tSize {Size}\n\tDepth {Depth}\n\n");

            // Top Layer
            bld.Append("Visible Layer :\n");
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    bld.Append($"{TopLayer[x, y]}\t\t");
                }
                bld.Append('\n');
            }
            bld.Append("------------------------------------------------\n");

            // Tunnels
            bld.Append("Tunnels :\n");
            Bloc[,][] tunnels = new Bloc[Size, Size][];
            for (int d = 0; d < Depth; d++)
            {
                bld.Append($"Layer : {d + 1} \n");
                for (int x = 0; x < Size; x++)
                {
                    for (int y = 0; y < Size; y++)
                    {
                        if (tunnels[x, y] == null)
                        {
                            tunnels[x, y] = [.. Tunnels[x, y]];
                        }
                        StringBuilder tunnel = new(tunnels[x, y].Length > d ? tunnels[x, y][d].ToString() : "Empty");
                        tunnel.Append("\t\t");
                        bld.Append(tunnel);
                    }
                    bld.Append('\n');
                }
                bld.Append("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
            }

            // Strates
            bld.Append("Strates :\n");
            foreach (Strate s in Strates)
            {
                bld.Append(s);
                bld.Append('\n');
            }

            // Mineurs
            bld.Append("Mineurs :\n\n");
            foreach (Mineur m in Mineurs)
            {
                bld.Append(m);
                bld.Append('\n');
            }

            return bld.ToString();
        }
    }
}