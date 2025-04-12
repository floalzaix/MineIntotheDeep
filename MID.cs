using MineIntoTheDeep.Helpers;
using MineIntoTheDeep.Models;

namespace MineIntoTheDeep
{
    public class MID
    {
        // Instance variables
        public Guid Id { get; init; }
        public string Name { get; init; }
        public int NbOfPlayer { get; init; }
        public Carte Carte { get; init; }
        public List<Joueur> Joueurs { get; init; } = [];
        public Tours? Tours { get; set; }
        public bool Started { get; set; } = false;
        public List<string> Clients { get; init; } = []; 

        // Constructors
        public MID(Guid id, string name, int nbOfPlayer) {
            Id= id;
            Name = name;
            NbOfPlayer = nbOfPlayer;
            Carte = new(id, nbOfPlayer);
        }
        public MID(Guid id, string name, int nbOfPlayer, int seed) {
            RandomGenerator.SetSeed(seed);
            Id= id;
            Name = name;
            NbOfPlayer = nbOfPlayer;
            Carte = new(id, nbOfPlayer);
        }

        //
        //  Functions
        //

        /// <summary>
        /// Creates a player to this game
        /// </summary>
        /// <param name="name"> The player's game </param>
        /// <returns> The number of the player in this game </returns>
        public int CreatePlayer(string name) {
            int num = Joueurs.Count;
            Joueur joueur = new (num, name, Carte);
            Joueurs.Add(joueur);
            return num;
        }

        /// <summary>
        /// Starts the game by creating Tours
        /// </summary>
        public void StartWithoutTimer() {
            Tours = new(Id, Carte, [.. Joueurs]);
            Started = true;
        }

        /// <summary>
        /// Starts the game by creating Tours + Timer
        /// </summary>
        public void StartWithTimer() {
            Tours = new (Id, Carte, [.. Joueurs]);
            Tours.Start();
            Started = true;
        }

        /// <summary>
        /// Stops the game
        /// </summary>
        public void Stop() {
            Started = false;
            Tours?.Stop();
        }
    }
}