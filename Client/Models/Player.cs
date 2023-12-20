using System.Collections.Generic;

namespace Client.Models;

public class Player
{
    public string Username { get; set; }
    public List<ShowCard> CardsInPlay { get; set; }
}