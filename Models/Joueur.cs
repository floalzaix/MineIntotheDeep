using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineIntoTheDeep.Models.Blocs;
using MineIntoTheDeep.Models.Pioches;
using MineIntoTheDeep.models;

namespace MineIntoTheDeep.Models
{
    public class Joueur
    {
        // Class Variables
        public static readonly List<Joueur> Joueurs = [];
        public static readonly int NB_MAX_MINEURS = 3;

        // Instance variables
        public string Name { get; init; }
        public Carte Carte { get; init; }
        public List<Mineur> Mineurs { get; }
        public int Score { get; set; }
        public bool Saboted { get; set; }
        public int Actions { get; set; }

        // Constructors
        public Joueur(string name, Carte carte, int score = 0)
        {
            Name = name;
            Carte = carte;
            Score = score;
            Mineurs = [];
            Saboted = false;
            Actions = 0;

            // Default miner
            Embaucher();

            // Registers it self
            Joueurs.Add(this);
        }

        //
        //  Functions
        //

        /// <summary>
        /// Adds a miner do the default position 0, 0. If there is one there then 1, 0 same until Size, Size
        /// </summary>
        /// <exception cref="InvalidOperationException"> If there is no space on the carte </exception>
        /// <returns> True if success and false else </returns>
        public bool Embaucher()
        {
            if (Mineurs.Count >= NB_MAX_MINEURS) {
                return false;
            }

            int x = 0, y = 0;
            while (Carte.MineurThere(x, y))
            {
                x += 1;
                if (x >= Carte.Size)
                {
                    x = 0;
                    y += 1;
                    if (y >= Carte.Size)
                    {
                        throw new InvalidOperationException("Can not add a miner there is no space on the carte !");
                    }
                }
            }

            Bloc blocUnder = Carte.GetBlocUnder(x, y);
            Mineur mineur = new(this, blocUnder);

            Mineurs.Add(mineur);
            Carte.Mineurs.Add(mineur);

            return true;
        }

        /// <summary>
        /// Removes a miner
        /// </summary>
        /// <param name="index"> The index of the miner </param>
        /// <returns> True if the function succeeded false otherwise </returns>
        public bool Retirer(int index)
        {
            try
            {
                Mineur mineur = Mineurs[index];
                Mineurs.Remove(mineur);
                Carte.Mineurs.Remove(mineur);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Moves the miner. 
        /// </summary>
        /// <param name="index"> The miner's index </param>
        /// <param name="x"> The new abscisse </param>
        /// <param name="y"> The new ordonnee </param>
        /// <returns> True if success false otherwise </returns>
        public bool Deplacer(int index, int x, int y)
        {
            if (!(0 <= index && index < Mineurs.Count && 0 <= x && x < Carte.Size && 0 <= y && y < Carte.Size))
            {
                return false;
            }

            try
            {
                if (!Carte.MineurThere(x, y))
                {
                    Bloc newBloc = Carte.GetBlocUnder(x, y);
                    Mineur mineur = Mineurs[index];

                    mineur.BlocUnder = newBloc;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Improves the grade of the pioche of a miner.
        /// </summary>
        /// <param name="index"> The index of the miner </param>
        /// <returns> True if it succeded false otherwise </returns>
        public bool Ameliorer(int index)
        {
            try
            {
                Mineur mineur = Mineurs[index];
                mineur.Pioche = mineur.Pioche switch
                {
                    PiocheBasique => new PiocheFer(),
                    PiocheFer => new PiocheDiamant(),
                    PiocheDiamant => throw new InvalidOperationException("Cannot improve a pioche which is diamant !"),
                    _ => throw new ArgumentOutOfRangeException(nameof(index), "The pioche of the miner is not a valid one !")
                };
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Undermine a player by makin its miners skip theirs turn.
        /// </summary>
        /// <param name="index"> The player </param>
        /// <returns> True if success false else </returns>
        public bool Saboter(int index)
        {
            try
            {
                Joueurs.Where(j => j.Carte == Carte).ToList()[index].Saboted = true;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets all the scores of the player this way : SCORES|360|0|100|... Where 360 is the 
        /// score of the player 0 ..
        /// </summary>
        /// <returns> The scores the way it has been described in the summary </returns>
        public string GetScores()
        {
            StringBuilder bld = new("SCORES");

            foreach (Joueur j in Joueurs.Where(j => j.Carte == Carte))
            {
                bld.Append('|');
                bld.Append(j.Score);
            }

            return bld.ToString();
        }

        /// <summary>
        /// Scans the bloc at x y and the 3 below
        /// </summary>
        /// <param name="x"> The x coord of the block to scan </param>
        /// <param name="y"> the y coord of the block to scan </param>
        /// <returns> Something like "SONAR|80|450|-1|-1" </returns>
        public string Sonar(int x, int y)
        {
            StringBuilder b = new("SONAR");

            // Getting the top layer value
            int value = Carte.TopLayer[x, y].GetTotalValue();
            b.Append('|');
            b.Append(value);

            // Getting the 3 nexts from the tunnels
            Bloc[] tunnel = [.. Carte.Tunnels[x, y]];
            for (int i = 0; i < 3; i++)
            {
                if (value != -1)
                {
                    if (i >= tunnel.Length)
                    {
                        value = -1;
                    }
                    else
                    {
                        value = tunnel[i].GetTotalValue();
                    }
                    b.Append('|');
                    b.Append(value);
                }
            }

            return b.ToString();
        }

        public bool Action(string action) {
            if (Actions > 0) {
                try {
                    string[] elements = action.Split('|');
                    switch (elements[0]) {
                        case "DEPLACER":
                            Deplacer(int.Parse(elements[1]), int.Parse(elements[2]), int.Parse(elements[3]));
                            break;
                        case "RETIRER":
                            Retirer(int.Parse(elements[1]));
                            break;
                        case "EMBAUCHER":
                            Embaucher();
                            break;
                        case "AMELIORER":
                            Ameliorer(int.Parse(elements[1]));
                            break;
                        case "SABOTER":
                            Saboter(int.Parse(elements[1]));
                            break;
                        case "SCORES":
                            GetScores();
                            break;
                        case "SONAR":
                            Sonar(int.Parse(elements[1]), int.Parse(elements[2]));
                            break;
                        default:
                            return false;
                }
                } catch (Exception) {
                    return false;
                }
                Actions--;

                return true;
            } 
            return false;
        }

        // Overrides
        public override string ToString()
        {
            return $"Joueur : {Name} Mineurs {Mineurs} Score {Score} Saboted {Saboted} Actions {Actions}";
        }
    }
}