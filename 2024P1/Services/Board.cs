using P1.Models;

namespace P1.Services;

public interface IBoard
{
    Dictionary<(int x, int y), Card> Cards { get; }
    IEnumerable<(int x, int y)> GetCardAdjacentPositions();
    void Add(Card card, (int x, int y) pos);
    void Remove((int x, int y) position);
}

public class Board: IBoard
{
    public Board()
    {
        Cards = new();
    }
    public Dictionary<(int x, int y), Card> Cards { get; private set; }
    public void Add(Card card, (int x, int y) pos)
    {
        Cards.Add(pos, card);
    }

    public void Remove((int x, int y) position)
    {
        Cards.Remove(position);
    }

    public IEnumerable<(int x, int y)> GetCardAdjacentPositions() =>
        Cards.Keys
            .SelectMany(GetAdjacent)
            .Distinct();
    
    
    private static IEnumerable<(int x, int y)> GetAdjacent((int x, int y) pos) => [
        pos with { y = pos.y + 1 },
        pos with { y = pos.y - 1 },
        pos with { x = pos.x + 1 },
        pos with { x = pos.x - 1 }
    ];
}