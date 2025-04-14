using System.Numerics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using MineIntoTheDeep.Models;
using MineIntoTheDeep.Models.Blocs;

namespace MineIntoTheDeep.Views;

class GameModel : PageModel {
    // Instance variables
    public string? Message { get; set; }
    public Guid GameId { get; set; }
    public List<Joueur>? Joueurs { get; set; }
    public int? CurrentPlayer { get; set; }
    public Carte? Carte { get; set; }
    public string? PreQuery { get; set; }
    public string? ResQuery { get; set; }

    // Bind parameters

    public IActionResult OnGet(string? preQuery, string query) {
        if (!Update()) {
            return Redirect("/");
        }

        PreQuery = preQuery;

        if (query != null && CurrentPlayer != null) {
            string res = "NOK";

            string[] args = query.Split('|');
            string action = args[0];
            
            if (PreQuery != null) {
                string[] preArgs = PreQuery.Split('|');
                string preAction = preArgs[0];

                const string MINEUR = "MINEUR_OWN";
                switch (action) {
                    case "CASE":
                        if (preAction == MINEUR) {
                            res = MIDApi.Query(GameId, (int) CurrentPlayer, $"DEPLACER|{preArgs[1]}|{args[1]}|{args[2]}");
                        }
                        break;
                    case "RETIRER":
                        if (preAction == MINEUR) {
                            res = MIDApi.Query(GameId, (int) CurrentPlayer, $"RETIRER|{preArgs[1]}");
                        }
                        break;
                    case "SABOTER":
                        if (preAction == "JOUEUR") {
                            res = MIDApi.Query(GameId, (int) CurrentPlayer, $"SABOTER|{preArgs[1]}");
                        }
                        break;
                    case "AMELIORER":
                        if (preAction == MINEUR) {
                            res = MIDApi.Query(GameId, (int) CurrentPlayer, $"AMELIORER|{preArgs[1]}");
                        }
                        break;
                    case "SONAR":
                        if (preAction == "CASE") {
                            res = MIDApi.Query(GameId, (int) CurrentPlayer, $"SONAR|{preArgs[1]}|{preArgs[2]}");
                        } else if (preAction == MINEUR || preAction == "MINEUR") {
                            Bloc? bloc = Joueurs?.First(j => j.Num == CurrentPlayer).Mineurs[int.Parse(preArgs[1])].BlocUnder;
                            if (bloc != null) {
                                res = MIDApi.Query(GameId, (int) CurrentPlayer, $"SONAR|{bloc.X}|{bloc.Y}");
                            } else {
                                res = "NOK";
                            }
                        }
                        break;
                    default:
                        break;
                }
            } 

            switch (action) {
                case "EMBAUCHER":
                    res = MIDApi.Query(GameId, (int) CurrentPlayer, "EMBAUCHER");
                    break;
                case "TOUR_FINI":
                    res = MIDApi.Query(GameId, (int) CurrentPlayer, "TOUR_FINI");
                    break;
                default:
                    break;
            }
            
            PreQuery = query;

            if (res != "NOK") {
                PreQuery = null;
            }

            ResQuery = res;
        }

        if (!Update()) {
            return Redirect("/");
        }

        return Page();
    }

    public IActionResult OnPost() {
        if (!Update()) {
            return Redirect("/");
        }

        return Page();
    }

    /// <summary>
    /// Updates the instance variables do keep updated the page
    /// </summary>
    /// <returns> True if success false otherwise </returns>
    public bool Update() {
        string? gameId = HttpContext.Session.GetString("GameId");
        if (gameId == null) {
            return false;
        }

        GameId = Guid.Parse(gameId);

        if (!Started()) {
            return false;
        }

        Joueurs = MIDApi.Games[GameId].Joueurs;
        Carte = MIDApi.Games[GameId].Carte;
        CurrentPlayer = MIDApi.Games[GameId].Tours?.GetWhosTurnItIs();

        return true;
    }

    /// <summary>
    /// Tests if the game is already started
    /// </summary>
    /// <returns> True if already strated false else </returns>
    public bool Started() {
        return MIDApi.Games[GameId].Started;
    }
}