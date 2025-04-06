using MineIntoTheDeep.Models.Blocs;
using MineIntoTheDeep.Models.Pioches;

namespace MineIntoTheDeep.Models
{
    public class Mineur
    {
        // Instance variables
        public Joueur Joueur { get; init; }
        public Bloc BlocUnder { get; set; }
        public Pioche Pioche { get; set; }

        // Constructors
        public Mineur(Joueur joueur, Bloc blocUnder, Pioche pioche) {
            Joueur = joueur;
            BlocUnder = blocUnder;
            Pioche = pioche;
        }
        public Mineur(Joueur joueur, Bloc blocUnder) {
            Joueur = joueur;
            BlocUnder = blocUnder;

            Pioche = new PiocheBasique();
        }

        //
        // Functions
        // 
        
        /// <summary>
        /// Mines the bloc under the player. Add the reward to the player's score.
        /// </summary>
        public void Mine() {
            if (Joueur.Saboted) {
                Joueur.Saboted = false;
            } else {
                Joueur.Score += BlocUnder.Mine(Pioche);
            }
        }

        // Overrides
        public override string ToString() {
            return $"Mineur :\n {Joueur}\nBlocUnder {BlocUnder}\n {Pioche}";
        }
    }
}