using P1.Models;

namespace P1.Services;

public interface IHand
{
    List<Card> Cards { get; }
    void Remove(int id);
}

public class Hand : IHand
{
    public Hand(List<Card> cards)
    {
        Cards = cards;
    }
    public List<Card> Cards { get; }
    public void Remove(int id)
    {
        Cards.Remove(Cards.Single(c => c.Id == id));
    }
}