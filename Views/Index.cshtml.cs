using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MineIntoTheDeep.Views;

public class IndexModel() : PageModel
{
    // Instance variables
    public List<Guid>? Games { get; set; } = [.. MIDApi.Games.Keys];
    public string Message { get; set; } = "";

    // Bind properties
    [BindProperty]
    public Guid? GameSelected {get; set; }
    [BindProperty]
    public string? Button { get; set; }
    [BindProperty]
    public string? Name { get; set; }
    [BindProperty]
    public int? NbOfPlayer { get; set; }
    [BindProperty]
    public int? Seed { get; set; }


    public IActionResult OnGet()
    {
        HttpContext.Session.Remove("GameId");
        return Page();
    }

    public IActionResult OnPost() {
        // Creating the game 
        if (Button == "Nouvelle partie") {
            if (Name != null && NbOfPlayer != null && NbOfPlayer >= 1 && NbOfPlayer <= 9) {
                GameSelected = MIDApi.CreateGame(Name, (int) NbOfPlayer, Seed);
            } else {
                Message = "Erreur : Le nom de partie est vide ou le nombre de joueurs n'est pas entre 1 et 9";
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
