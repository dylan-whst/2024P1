using P1.Models;

namespace P1.Services;

public interface IGameService
{
    List<Card> DrawHand();
}

public class StubGameService : IGameService
{
    public List<Card> DrawHand()
    {
         return [
            new Card() { Id = 1, Name = "Card a", Letter = "a", Place = "Hand" },
            new Card() { Id = 2, Name = "Card b", Letter = "b", Place = "Hand" },
            new Card() { Id = 3, Name = "Card c", Letter = "c", Place = "Hand" }
        ];
    }
}