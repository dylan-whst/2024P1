using P1.Models;

namespace P1.Services;

public interface IHandService
{
    List<Card> Cards { get; }
    int HandSize { get; set; }
    Card Remove(int id);
    void Add(Card card);
    void Reset();
}

public class HandService : IHandService
{
    public HandService(List<Card> cards)
    {
        Cards = cards;
    }
    public List<Card> Cards { get; private set; }
    public int HandSize { get; set; } = 5;

    public HandService SetHandSize(int size)
    {
        HandSize = size;
        return this;
    }

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

    public void Reset()
    {
        Cards = new List<Card>();
    }
}

