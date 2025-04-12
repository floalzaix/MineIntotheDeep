using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using MineIntoTheDeep.Models;

namespace MineIntoTheDeep;

public class MIDApi : Hub
{
    // Instance variables
    public static Dictionary<Guid, MID> Games { get; } = [];
    public static IHubContext<MIDApi> HUBCONTEXT;

    // Constructors
    public MIDApi(IHubContext<MIDApi> hubContext) {
        HUBCONTEXT = hubContext;
    }

    //
    //  Functions
    //

    /// <summary>
    /// Creates a game and stores it, server's side (meant to)
    /// </summary>
    /// <param name="name"> Name of the game </param>
    /// <param name="nbOfPlayer"> The number of the player in the game </param>
    /// <param name="seed"> The seed to generate the same carte if the same nbOfPlayer </param>
    /// <returns> The Guid of the game so that it maybe found in the </returns>
    public static Guid CreateGame(string name, int nbOfPlayer, int? seed)
    {
        Guid id = Guid.NewGuid();
        MID mid = new(id, name, nbOfPlayer);
        Games[id] = mid;
        return id;
    }

    /// <summary>
    /// Create a player to play in a game (calls the game function to do it)
    /// </summary>
    /// <param name="gameId"> The game's Id</param>
    /// <param name="name"> The player's name </param>
    /// <returns> The player's num in the game (relative to the game) -1 if anything went wrong </returns>
    public static int CreatePlayer(Guid gameId, string name)
    {
        try
        {
            return Games[gameId].CreatePlayer(name);
        }
        catch (Exception)
        {
            return -1;
        }
    }

    /// <summary>
    /// Starts the game. Meaning it creates a Tours which handles the turns
    /// </summary>
    /// <param name="gameId"> The game's id </param>
    /// <param name="timer"> With a timer between each turn </param>
    /// <returns> "OK" if everythin went rightfuly "NOK" otherwise </returns>
    public static string StartGame(Guid gameId, bool timer = false)
    {
        try
        {
            MID game = Games[gameId];
            if (game.Joueurs.Count == game.NbOfPlayer)
            {
                if (timer)
                {
                    game.StartWithTimer();
                }
                else
                {
                    game.StartWithoutTimer();
                }
                game.Tours.OnNext += OnNext;
                game.Carte.OnEnd += OnEnd;

                HUBCONTEXT.Clients.Group(gameId.ToString()).SendAsync("MESSAGE", "REFRESH");
            }
            else
            {
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
    /// Makes a query to the game
    /// </summary>
    /// <param name="gameId"> The game's id </param>
    /// <param name="playerNum"> The player's number </param>
    /// <param name="query"> The action like ACTION|arg|arg|arg ... </param>
    /// <returns> "OK" or another feedback string or "NOK" if something went wrong </returns>
    public static string Query(Guid gameId, int playerNum, string query)
    {
        string ret = "OK";
        string action = query.Split("|")[0];
        try
        {
            if (action == "TOUR_FINI")
            {
                Games[gameId].Tours?.Next();
            }
            else
            {
                ret = Games[gameId].Joueurs[playerNum].Action(query);
            }
            HUBCONTEXT.Clients.Group(gameId.ToString()).SendAsync("MESSAGE", "REFRESH");
        }
        catch (Exception)
        {
            return "NOK";
        }
        return ret;
    }

    //
    // Players' HUB
    //

    public override Task OnConnectedAsync()
    {
        Clients.Caller.SendAsync("MESSAGE", "Connection réussi à l'api !");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Clients.Caller.SendAsync("MESSAGE", "Déconnecté de l'api !");
        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Creates a game
    /// </summary>
    /// <param name="name"> The name of the game </param>
    /// <param name="nbOfPlayer"> the number of players </param>
    /// <param name="seed"> The seed of generation of the carte </param>
    /// <sends> The id of the game like : GAME|id </sends>
    public async Task CreateGameAsync(string name, int nbOfPlayer, int? seed)
    {
        Guid id = CreateGame(name, nbOfPlayer, seed);
        await Clients.Caller.SendAsync("MESSAGE", $"GAME|{id}");
    }

    /// <summary>
    /// Sends the games to the client
    /// </summary>
    /// <sends> The client in the format : GAMES|id|name|... </sends>
    public async Task GetGamesAsync()
    {
        StringBuilder b = new("GAMES");
        foreach (KeyValuePair<Guid, MID> pair in Games)
        {
            b.Append('|');
            b.Append(pair.Key);
            b.Append(';');
            b.Append(pair.Value.Name);
        }
        await Clients.Caller.SendAsync("MESSAGE", b.ToString());
    }

    /// <summary>
    /// Joins the game by creating a player and sending the num of the player
    /// </summary>
    /// <param name="gameId"> The ID of the game </param>
    /// <param name="name"> The name of the team or player </param>
    /// <sends> The num of the player or ERREUR with the query (ERREUR|explanation...) </sends>
    public async Task JoinGameAsync(string gameId, string name)
    {
        try
        {
            Guid id = Guid.Parse(gameId);
            if (!Games[id].Started)
            {
                int num = CreatePlayer(id, name);
                await Clients.Caller.SendAsync("MESSAGE", $"DEBUT_PARTIE|{num}");

                string clientId = Context.ConnectionId;
                if (clientId != null)
                {
                    Games[id].Clients.Add(clientId);
                    await Groups.AddToGroupAsync(clientId, gameId);
                } else {
                    throw new ArgumentNullException(clientId, "Client ID est null bizarre !");
                }
                
                await Clients.Group(gameId).SendAsync("MESSAGE", "REFRESH");
            }
            else
            {
                await Clients.Caller.SendAsync("MESSAGE", $"Game already started !");
            }
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("MESSAGE", "ERREUR|" + e.Message);
        }
    }

    public async Task ReJoinGameAsync(string gameId, int playerNum)
    {
        try
        {
            Guid id = Guid.Parse(gameId);

            string clientId = Context.ConnectionId;
            if (clientId != null)
            {
                Games[id].Clients[playerNum] = clientId;
                await Groups.AddToGroupAsync(clientId, gameId);
            } else {
                throw new ArgumentNullException(gameId, "Client ID est null bizarre !");
            }
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("MESSAGE", "ERREUR|" + e.Message);
        }
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    /// <param name="gameId"> The id of the game </param>
    /// <sends> Ok if success NOK otherwise or ERREUR with the query (ERREUR|explanation...)  </send>
    public async Task StartGameAsync(string gameId)
    {
        try
        {
            string res = StartGame(Guid.Parse(gameId));
            await Clients.Group(gameId).SendAsync("MESSAGE", "REFRESH");
            await Clients.Caller.SendAsync("MESSAGE", res);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("MESSAGE", "ERREUR|" + e.Message);
        }
    }

    /// <summary>
    /// Makes a query to a game in the server and refresh for the other players of the game 
    /// </summary>
    /// <param name="gameId"> The game ID </param>
    /// <param name="playerNum"> The num of the player </param>
    /// <param name="query"> The query </param>
    /// <sends> The result of the query OK NOK or ERREUR with the query (ERREUR|explanation...) </sends>
    public async Task QueryAsync(string gameId, int playerNum, string query)
    {
        try
        {
            string res = Query(Guid.Parse(gameId), playerNum, query);
            await Clients.Group(gameId).SendAsync("MESSAGE", "REFRESH");
            await Clients.Caller.SendAsync("MESSAGE", res);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("MESSAGE", "ERREUR|" + e.Message);
        }
    }

    //
    //  Events
    //

    /// <summary>
    /// Catchs the event of the next turn
    /// </summary>
    /// <param name="obj"> The Tours object </param>
    /// <param name="message"> DEBUT_TOUR|14 </param>
    public static async void OnNext(object? obj, string message)
    {
        try
        {
            if (obj != null)
            {
                Tours tours = (Tours)obj;
                await HUBCONTEXT.Clients.Group(tours.GameId.ToString()).SendAsync("MESSAGE", "REFRESH");
                await HUBCONTEXT.Clients.User(Games[tours.GameId].Clients[tours.GetWhosTurnItIs()]).SendAsync("MESSAGE", message);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Problème lors de l'actualisation dût au next tour : " + e.Message);
        }
    }

    /// <summary>
    /// Catchs the event of the end when a mineurs digs too deep
    /// </summary>
    /// <param name="obj"> The Tours object </param>
    /// <param name="message"> DEBUT_TOUR|14 </param>
    public static async void OnEnd(object? obj, string message)
    {
        try
        {
            if (obj != null)
            {
                Tours tours = (Tours)obj;
                await HUBCONTEXT.Clients.Group(tours.GameId.ToString()).SendAsync("MESSAGE", "REFRESH");
                await HUBCONTEXT.Clients.Group(tours.GameId.ToString()).SendAsync("MESSAGE", "END");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Problème lors de l'actualisation dût à la fin : " + e.Message);
        }
    }
}