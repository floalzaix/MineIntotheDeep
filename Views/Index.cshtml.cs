using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MineIntoTheDeep.Views;

public class IndexModel() : PageModel
{
    // Instance variables
    public List<Guid>? Games { get; set; }
    public string Message { get; set; } = "";

    // Bind properties
    [BindProperty]
    public Guid? GameSelected {get; set; }
    [BindProperty]
    public string? Button { get; set; }
    [BindProperty]
    public int? Seed { get; set; }
    [BindProperty]
    public int? NbOfPlayer { get; set; }


    public void OnGet()
    {
        Games = [.. MIDApi.Games.Keys];
    }

    public IActionResult OnPost() {
        // Creating the game 
        if (Button == "Nouvelle partie") {
            if (NbOfPlayer != null && NbOfPlayer >= 1 && NbOfPlayer <= 9) {
                GameSelected = MIDApi.CreateGame((int) NbOfPlayer, Seed);
            } else {
                Message = "Erreur : Le nombre de joueurs doit être entre 1 et 9";
                return Page();
            }
        }

        // Storing the game id
        if (GameSelected == null) {
            Message = "Erreur : aucune partie sélectionnée";
            return Page();
        }

        HttpContext.Session.SetString("GameId", ((Guid) GameSelected).ToString());

        return Redirect("/Connection");
    }
}
