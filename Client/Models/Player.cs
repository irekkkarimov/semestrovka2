using System.Collections.Generic;
using FluxxGame.Models;

namespace Client.Models;

public class Player
{
    public string Username { get; set; }
    public List<ShowCard> CardsInPlay { get; set; }
}