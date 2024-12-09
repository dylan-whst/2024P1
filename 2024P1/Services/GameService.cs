using P1.Models;

namespace P1.Services;

public interface IGameService
{
    List<CardVM> DrawHand();
}

public class StubGameService : IGameService
{
    public List<CardVM> DrawHand()
    {
         return [
            new CardVM() { Id = 1, Name = "Card a", Text = "a", Place = CardPlace.InHand },
            new CardVM() { Id = 2, Name = "Card b", Text = "b", Place = CardPlace.InHand },
            new CardVM() { Id = 3, Name = "Card c", Text = "c", Place = CardPlace.InHand }
        ];
    }
}