using MineIntoTheDeep.Models;

namespace MineIntoTheDeep;

public static class MIDApi
{
    // Instance variables
    private static readonly Dictionary<Guid, MID> games = [];

    //
    //  Functions
    //

    /// <summary>
    /// Creates a game and stores it, server's side (meant to)
    /// </summary>
    /// <param name="nbOfPlayer"> The number of the player in the game </param>
    /// <param name="seed"> The seed to generate the same carte if the same nbOfPlayer </param>
    /// <returns> The Guid of the game so that it maybe found in the </returns>
    public static Guid CreateGame(int nbOfPlayer, int seed)
    {
        Guid id = Guid.NewGuid();
        MID mid = new(nbOfPlayer);
        games[id] = mid;
        return id;
    }

    /// <summary>
    /// Gets the game given the id
    /// </summary>
    /// <param name="id"> The guid of the game </param>
    /// <returns> The game or null if doesn't exist </returns>
    public static MID? GetGameById(Guid id) {
        return games[id] ?? null;
    }

    /// <summary>
    /// Create a player to play in a game (calls the game function to do it)
    /// </summary>
    /// <param name="gameId"> The game's Id</param>
    /// <param name="name"> The player's name </param>
    /// <returns> The player's num in the game (relative to the game) -1 if anything went wrong </returns>
    public static int CreatePlayer(Guid gameId, string name) {
        try {
            return games[gameId].CreatePlayer(name);
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
            MID game = games[gameId];
            if (timer) {
                game.StartWithTimer();
            } else {
                game.StartWithoutTimer();
            }
        } catch (Exception) {
            return "NOK";
        }

        return "OK";
    }

    public static string Query(Guid gameId, int playerNum, string query) {
        string ret = "OK";
        string action = query.Split("|")[0];
        try {
            if (action == "TOUR_FINI") {
                games[gameId].Tours?.Next();
            } else {
                games[gameId].Joueurs[playerNum].Action(query);
            }
        } catch (Exception) {
            return "NOK";
        }
        return ret;
    }
}