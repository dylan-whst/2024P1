using System.Text.RegularExpressions;
using _2024P1.Game;
using P1.Models;

namespace P1.Services;

public interface IGameBoardViewModel
{
    List<(int x, int y)> BoardDropZonePositions { get; }
    bool IsCardAtPosition((int x, int y) pos);
    List<Card> Cards { get; }
    void MoveCard(Card card, string place);
}

public class GameBoardViewModel : IGameBoardViewModel
{
    private IGameService _gameService;
    public List<(int x, int y)> BoardDropZonePositions { get; } = [(0, 0)];

    public List<Card> Cards { get; }
    
    private List<Card> BoardCards => Cards.Where(card => card.Place == "Board").ToList();
    
    public GameBoardViewModel(IGameService gameService)
    {
        _gameService = gameService;
        Cards = _gameService.DrawHand();
    }

    public void MoveCard(Card card, string place)
    {
        var boardPos = GetBoardPosition(place);

        foreach (var adjacentPos in GetAdjacent(boardPos)
                     .Where(adjacentPos => !BoardDropZonePositions.Contains(adjacentPos)))
            BoardDropZonePositions.Add(adjacentPos);
        
        card.Place = place;
    }
    
    public bool IsCardAtPosition((int x, int y) pos)
    {
        return BoardCards.Any(card => GetBoardPosition(card.Place) == pos);
    }

    private static (int x, int y) GetBoardPosition(string place)
    {
        var match = Regex.Match(place, @"\((-?\d+),(-?\d+)\)");
        return (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
    }

    private static List<(int x, int y)> GetAdjacent((int x, int y) pos) => [
        pos with { y = pos.y + 1 },
        pos with { y = pos.y - 1 },
        pos with { x = pos.x + 1 },
        pos with { x = pos.x - 1 }
    ];
}