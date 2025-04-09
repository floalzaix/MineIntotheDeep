using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MineIntoTheDeep.Views;

public class IndexModel() : PageModel
{
    // Instance variables
    public List<Guid> Games { get; set; } = [.. MIDApi.Games.Keys];
    public string Message { get; set; } = "";

    // Bind properties
    [BindProperty]
    public Guid GameSelected {get; set; }
    [BindProperty]
    public string? Button { get; set; }
    [BindProperty]
    public int Seed { get; set; } = 1;
    [BindProperty]
    public int? NbOfPlayer { get; set; }


    public void OnGet()
    {
        
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
        HttpContext.Session.SetString("GameId", GameSelected.ToString());

        return Redirect("/");
    }
}
