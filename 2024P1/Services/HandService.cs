using P1.Models;

namespace P1.Services;

public interface IHandService
{
    List<Card> Cards { get; }
    Card Remove(int id);
}

public class HandService : IHandService
{
    public HandService(List<Card> cards)
    {
        Cards = cards;
    }
    public List<Card> Cards { get; }
    public Card Remove(int id)
    {
        var cardToRemove = Cards.Single(c => c.Id == id);
        Cards.Remove(cardToRemove);
        return cardToRemove;
    }
}