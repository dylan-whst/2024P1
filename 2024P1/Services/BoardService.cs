using P1.Models;

namespace P1.Services;

public interface IBoardService
{
    Dictionary<(int x, int y), Card> Cards { get; }
    IEnumerable<(int x, int y)> GetCardAdjacentPositions();
    void Add(Card card, (int x, int y) pos);
    Card Remove((int x, int y) position);
    public bool WouldCardBreakBoardIfGone((int x, int y) pos);
    bool WouldCardBreakBoardIfMoved((int x, int y) cardPos, (int x, int y) destPos);
}

public class BoardService: IBoardService
{
    public BoardService()
    {
        Cards = new();
    }
    public Dictionary<(int x, int y), Card> Cards { get; private set; }
    public void Add(Card card, (int x, int y) pos)
    {
        Cards.Add(pos, card);
    }

    public Card Remove((int x, int y) position)
    {
        var cardToRemove = Cards[position];
        Cards.Remove(position);
        return cardToRemove;
    }

    public IEnumerable<(int x, int y)> GetCardAdjacentPositions() =>
        Cards.Keys
            .SelectMany(GetAdjacent)
            .Distinct();

    public bool WouldCardBreakBoardIfGone((int x, int y) pos)
    {
        var tempCardsCopy = Cards.ToDictionary(c => c.Key, c => c.Value);
        tempCardsCopy.Remove(pos);

        return IsHypotheticalBoardBroken(tempCardsCopy);
    }

    public bool WouldCardBreakBoardIfMoved((int x, int y) cardPos, (int x, int y) destPos)
    {
        var tempCardsCopy = Cards.ToDictionary(c => c.Key, c => c.Value);
        var card = tempCardsCopy[cardPos];
        tempCardsCopy.Remove(cardPos);
        tempCardsCopy.Add(destPos, card);
        
        return IsHypotheticalBoardBroken(tempCardsCopy);
    }

    /// <summary>
    /// Broken means that not every card is connected adjacently as a single unit
    /// </summary>
    private static bool IsHypotheticalBoardBroken(Dictionary<(int x, int y), Card> board)
    {
        // starting with some position, 'visit' each adjacent position recursively
        // at the end, if not every position was visited then the board is 'broken'
        List<(int x, int y)> notVisitedPositions = board.Keys.ToList();
        List<(int x, int y)> toVisitQueue = [notVisitedPositions.First()];

        int numVisited = 0;
        while (toVisitQueue.Count != 0)
        {
            foreach (var visiting in toVisitQueue.ToList())
            {
                numVisited++;
                toVisitQueue.Remove(visiting);
                notVisitedPositions.Remove(visiting);

                // add to visit queue all adjacent positions not visited yet or already in visit queue
                toVisitQueue.AddRange(
                    GetAdjacent(visiting)
                        .Where(adj => notVisitedPositions.Contains(adj) && !toVisitQueue.Contains(adj)));
            }
        }
        
        if (numVisited == board.Count)
            return true;
        else
            return false;
    }

    private static IEnumerable<(int x, int y)> GetAdjacent((int x, int y) pos) => [
        pos with { y = pos.y + 1 },
        pos with { y = pos.y - 1 },
        pos with { x = pos.x + 1 },
        pos with { x = pos.x - 1 }
    ];
}