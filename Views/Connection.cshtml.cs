using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MineIntoTheDeep.Models;

namespace MineIntoTheDeep.Views
{
    public class ConnectionModel() : PageModel
    {
        // Class variables
        public static readonly string GAME_PAGE = "/Game";

        // Instance variables
        public string Message { get; set; } = "";
        public Guid GameId { get; set; }
        public List<Joueur>? Joueurs { get; set; }

        // Bind parameters
        [BindProperty]
        public string? Button { get; set; }
        [BindProperty]
        public string? Name { get; set; }

        //
        //  Functions
        //

        public IActionResult OnGet() {
            if (!Update()) {
                return Redirect("/");
            }

            if (Started()) {
                return Redirect(GAME_PAGE);
            }

            return Page();
        }

        public IActionResult OnPost() {
            if (!Update()) {
                return Redirect("/");
            }

            if (Started()) {
                return Redirect(GAME_PAGE);
            }

            if (Button == "Nouveau joueur") {
                if (Name == null || Name == "") {
                    Message = "Erreur : Le nom doit être rempli";
                } else {
                    if (MIDApi.CreatePlayer(GameId, Name) == -1) {
                        Message = "Erreur dans la création du joueur veuillez réessayer";
                    }
                }
            } else if (Button == "Start") {
                if (MIDApi.StartGame(GameId, false) != "OK") {
                    Message = "Erreur dans le démarrage de la partie veuillez réessayer";
                } else {
                    return Redirect(GAME_PAGE);
                }
            } else if (Button == "Start avec temps") {
                if (MIDApi.StartGame(GameId, true) != "OK") {
                    Message = "Erreur dans le démarrage de la partie veuillez réessayer";
                } else {
                    return Redirect(GAME_PAGE);
                }
            }

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

            Joueurs = MIDApi.Games[GameId].Joueurs;

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
}