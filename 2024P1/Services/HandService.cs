using P1.Models;

namespace P1.Services;

public interface IHandService
{
    List<Card> Cards { get; }
    int HandSize { get; set; }
    int CardsCount { get; set; }
    Card Remove(int id);
    void Add(Card card);
}

public class HandService : IHandService
{
    public HandService(List<Card> cards)
    {
        Cards = cards;
    }
    public List<Card> Cards { get; }
    public int HandSize { get; set; }
    public int CardsCount { get; set; }

    public Card Remove(int id)
    {
        var cardToRemove = Cards.Single(c => c.Id == id);
        Cards.Remove(cardToRemove);
        return cardToRemove;
    }

    public void Add(Card card)
    {
        Cards.Add(card);
    }
}