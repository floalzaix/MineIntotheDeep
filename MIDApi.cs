using MineIntoTheDeep.Models;

namespace MineIntoTheDeep;

public static class MIDApi
{
    // Instance variables
    public static Dictionary<Guid, MID> Games { get; }= [];

    //
    //  Functions
    //

    /// <summary>
    /// Creates a game and stores it, server's side (meant to)
    /// </summary>
    /// <param name="nbOfPlayer"> The number of the player in the game </param>
    /// <param name="seed"> The seed to generate the same carte if the same nbOfPlayer </param>
    /// <returns> The Guid of the game so that it maybe found in the </returns>
    public static Guid CreateGame(int nbOfPlayer, int? seed)
    {
        Guid id = Guid.NewGuid();
        MID mid = new(nbOfPlayer);
        Games[id] = mid;
        return id;
    }

    /// <summary>
    /// Create a player to play in a game (calls the game function to do it)
    /// </summary>
    /// <param name="gameId"> The game's Id</param>
    /// <param name="name"> The player's name </param>
    /// <returns> The player's num in the game (relative to the game) -1 if anything went wrong </returns>
    public static int CreatePlayer(Guid gameId, string name) {
        try {
            return Games[gameId].CreatePlayer(name);
        } catch (Exception) {
            return -1;
        }
    }

    /// <summary>
    /// Starts the game. Meaning it creates a Tours which handles the turns
    /// </summary>
    /// <param name="gameId"> The game's id </param>
    /// <param name="timer"> With a timer between each turn </param>
    /// <returns> "OK" if everythin went rightfuly "NOK" otherwise </returns>
    public static string StartGame(Guid gameId, bool timer = false) {
        try {
            MID game = Games[gameId];
                if (game.Joueurs.Count == game.NbOfPlayer) {
                    if (timer) {
                    game.StartWithTimer();
                } else {
                    game.StartWithoutTimer();
                }
            } else {
                return "NOK";
            }
        } catch (Exception) {
            return "NOK";
        }

        return "OK";
    }

    /// <summary>
    /// Makes a query to the game
    /// </summary>
    /// <param name="gameId"> The game's id </param>
    /// <param name="playerNum"> The player's number </param>
    /// <param name="query"> The action like ACTION|arg|arg|arg ... </param>
    /// <returns> "OK" or another feedback string or "NOK" if something went wrong </returns>
    public static string Query(Guid gameId, int playerNum, string query) {
        string ret = "OK";
        string action = query.Split("|")[0];
        try {
            if (action == "TOUR_FINI") {
                Games[gameId].Tours?.Next();
            } else {
                ret = Games[gameId].Joueurs[playerNum].Action(query);
            }
        } catch (Exception) {
            return "NOK";
        }
        return ret;
    }
}