using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineIntoTheDeep.Models.Blocs;
using MineIntoTheDeep.Models.Pioches;

namespace MineIntoTheDeep.Models
{
    public class Joueur
    {
        // Class Variables
        public static readonly List<Joueur> Joueurs = [];
        public static readonly int NB_MAX_MINEURS = 3;

        // Instance variables
        public Guid Id { get; init; } = Guid.NewGuid();
        public int Num { get; init; }
        public string Name { get; init; }
        public Carte Carte { get; init; }
        public List<Mineur> Mineurs { get; }
        public int Score { get; set; }
        public bool Saboted { get; set; }
        public int Actions { get; set; }

        // Constructors
        public Joueur(int num, string name, Carte carte, int score = 0)
        {
            Num = num;
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
        /// <returns> OK if success and NOK else </returns>
        private string Embaucher()
        {
            if (Mineurs.Count >= NB_MAX_MINEURS)
            {
                return "NOK";
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

            return "OK";
        }

        /// <summary>
        /// Removes a miner
        /// </summary>
        /// <param name="index"> The index of the miner </param>
        /// <returns> OK if the function succeeded NOK otherwise </returns>
        private string Retirer(int index)
        {
            try
            {
                Mineur mineur = Mineurs[index];
                Mineurs.Remove(mineur);
                Carte.Mineurs.Remove(mineur);
            }
            catch (Exception)
            {
                return "NOK";
            }
            return "OK";
        }

        /// <summary>
        /// Moves the miner. 
        /// </summary>
        /// <param name="index"> The miner's index </param>
        /// <param name="x"> The new abscisse </param>
        /// <param name="y"> The new ordonnee </param>
        /// <returns> OK if success NOK otherwise </returns>
        private string Deplacer(int index, int x, int y)
        {
            if (!(0 <= index && index < Mineurs.Count && 0 <= x && x < Carte.Size && 0 <= y && y < Carte.Size))
            {
                return "NOK";
            }

            try
            {
                if (!Carte.MineurThere(x, y))
                {
                    Bloc newBloc = Carte.GetBlocUnder(x, y);
                    Mineur mineur = Mineurs[index];

                    mineur.BlocUnder = newBloc;
                } else {
                    return "NOK";
                }
            }
            catch (Exception)
            {
                return "NOK";
            }

            return "OK";
        }

        /// <summary>
        /// Improves the grade of the pioche of a miner.
        /// </summary>
        /// <param name="index"> The index of the miner </param>
        /// <returns> OK if it succeded NOK otherwise </returns>
        private string Ameliorer(int index)
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
                return "NOK";
            }

            return "OK";
        }

        /// <summary>
        /// Undermine a player by makin its miners skip theirs turn.
        /// </summary>
        /// <param name="index"> The player </param>
        /// <returns> OK if success NOK else </returns>
        private string Saboter(int index)
        {
            try
            {
                Joueurs.First(j => j.Num == index).Saboted = true;
            }
            catch (Exception)
            {
                return "NOK";
            }

            return "OK";
        }

        /// <summary>
        /// Gets all the scores of the player this way : SCORES|360|0|100|... Where 360 is the 
        /// score of the player 0 ..
        /// </summary>
        /// <returns> The scores the way it has been described in the summary </returns>
        private string GetScores()
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
        private string Sonar(int x, int y)
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

        /// <summary>
        /// Gets th current state of the map
        /// </summary>
        /// <returns> 
        /// 1|350|Fer|-1 where the first is depth the second the value of the bloc
        /// the thirs the type of the bloc and finally the last the joueur number if
        /// there is a mineur on the bloc.  
        /// </returns>
        private string GetCarte()
        {
            StringBuilder b = new();

            for (int x = 0; x < Carte.Size; x++)
            {
                for (int y = 0; y < Carte.Size; y++)
                {
                    // Get the Depth
                    b.Append(12 - Carte.Tunnels[x, y].Count);
                    b.Append(';');

                    // Get the value of the bloc
                    Bloc bloc = Carte.TopLayer[x, y];
                    b.Append(bloc.GetTotalValue());
                    b.Append(';');

                    // Type of the bloc
                    Ore? ore = bloc.Ore;
                    b.Append((ore != null) ? ore.Name.ToUpper() : "RIEN");
                    b.Append(';');

                    // Joueur's number on the case if there is a miner on it
                    Mineur? mineur = Carte.GetMineurThere(x, y);
                    Joueur? joueur = mineur?.Joueur;
                    b.Append(
                        (joueur != null) ? Joueurs.Where(j => j.Carte == Carte)
                                                  .ToList()
                                                  .IndexOf(joueur)
                                                  : -1
                    );
                    b.Append('|');
                }
            }

            return b.ToString();
        }

        /// <summary>
        /// The function which enables the player to act with any action according to this
        /// convention : SONAR|1|2
        /// </summary>
        /// <param name="action"> The action like "SONAR|1|2" </param>
        /// <returns> The return of the action executed </returns>
        public string Action(string action)
        {
            if (Actions > 0)
            {
                string ret;
                try
                {
                    ret = SelectAction(action);
                }
                catch (Exception)
                {
                    return "NOK";
                }

                if (ret != "NOK") {
                    Actions--;
                }

                return ret;
            }
            return "NOK";
        }

        /// <summary>
        /// Selects the action to play according to action
        /// </summary>
        /// <param name="action"> The action like DEPLACER|0|1|2 </param>
        /// <returns> The return of the action </returns>
        private string SelectAction(string action) {
            string[] elements = action.Split('|');
            string ret;
            switch (elements[0])
            {
                case "DEPLACER":
                    ret = Deplacer(int.Parse(elements[1]), int.Parse(elements[2]), int.Parse(elements[3]));
                    break;
                case "RETIRER":
                    ret = Retirer(int.Parse(elements[1]));
                    break;
                case "EMBAUCHER":
                    ret = Embaucher();
                    break;
                case "AMELIORER":
                    ret = Ameliorer(int.Parse(elements[1]));
                    break;
                case "SABOTER":
                    ret = Saboter(int.Parse(elements[1]));
                    break;
                case "SCORES":
                    ret = GetScores();
                    break;
                case "SONAR":
                    ret = Sonar(int.Parse(elements[1]), int.Parse(elements[2]));
                    break;
                case "CARTE":
                    ret = GetCarte();
                    break;
                default:
                    return "NOK";
            }
            return ret;
        }

        // Overrides
        public override string ToString()
        {
            StringBuilder b = new ($"Joueur : {Name} Score {Score} Saboted {Saboted} Actions {Actions}\n");

            // Mineurs
            b.Append("Mineurs :\n");
            foreach (Mineur m in Mineurs) {
                b.Append(m);
                b.Append('\n');
            }

            return b.ToString();
        }
    }
}