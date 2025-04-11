using System.Timers;

namespace MineIntoTheDeep.Models
{
    public class Tours
    {
        // Class variables
        public static readonly int NB_MAX_ACTIONS_PER_TURN = 2;
        public static readonly int TIME_PER_TURN = 60 * 1000;

        // Instance variables
        public Guid Id { get; init; } = Guid.NewGuid();
        public Carte Carte { get; init; }
        public Joueur[] Joueurs { get; init; }
        public int Index { get; set; }
        public Queue<int> Turns { get; set; }
        public int[] CurrentTurns { get; set; }

        private readonly System.Timers.Timer timer;

        // Constructors
        public Tours(Carte carte, Joueur[] joueurs, int index = 0)
        {
            Carte = carte;
            Joueurs = joueurs;
            Index = index;
            Turns = new Queue<int>(Enumerable.Range(0, Joueurs.Length));
            CurrentTurns = [.. Turns];

            Joueurs[CurrentTurns[Index]].Actions = NB_MAX_ACTIONS_PER_TURN;

            // Timer
            timer = new System.Timers.Timer(TIME_PER_TURN);
            timer.Elapsed += OnTimer;
        }

        //
        //  Functions
        //

        /// <summary>
        /// Switch to the next player by putting the precedent player actions to none
        /// and giving the current player the actions to play.
        /// </summary>
        public void Next()
        {
            bool en = timer.Enabled;
            if (en)
            {
                timer.Stop();
            }
            Joueurs[CurrentTurns[Index]].Actions = 0;
            Index++;
            if (Index >= Turns.Count)
            {
                Index = 0;
                Turns.Enqueue(Turns.Dequeue());
                CurrentTurns = [.. Turns];
                Carte.Miner();
            }
            Joueurs[CurrentTurns[Index]].Actions = NB_MAX_ACTIONS_PER_TURN;
            if (en)
            {
                timer.Start();
            }
        }

        public void OnTimer(object? src, ElapsedEventArgs e)
        {
            Next();
        }

        public void Start()
        {
            timer.Enabled = true;
            timer.Start();
        }

        public void Stop()
        {
            Joueurs[CurrentTurns[Index]].Actions = 0;
            timer.Stop();
            timer.Dispose();
        }

        public int GetWhosTurnItIs() {
            return Joueurs[CurrentTurns[Index]].Num;
        }

        //
        //  Overrides
        //

        public override string ToString()
        {
            return $"Players whos turn it is : {Joueurs[CurrentTurns[Index]]}";
        }
    }
}