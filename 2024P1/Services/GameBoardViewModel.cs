using System.Text.RegularExpressions;
using _2024P1.Game;
using MudBlazor;
using P1.Models;

namespace P1.Services;

public interface IGameBoardViewModel
{
    IEnumerable<(int x, int y)> BoardDropZonePositions { get; }
    bool IsCardAtPosition((int x, int y) pos);
    List<CardVM> Cards { get; }
    void MoveCard(CardVM cardVm, string moveToPlace);
    bool CanCardBeMovedToBoardPos(CardVM card, (int x, int y) destPos);
    bool CanCardBeMovedToHand(CardVM card);
}


public class GameBoardViewModel : IGameBoardViewModel
{
    private IBoardService _boardService;
    private IHandService _handService;
    
    public GameBoardViewModel(IBoardService boardService, IHandService handService)
    {
        _boardService = boardService;
        _handService = handService;
    }
    
    public List<CardVM> Cards =>
        _boardService.Cards.Select(c => CardToVm(c.Value, $"board-({c.Key.x},{c.Key.y})"))
            .Concat(_handService.Cards.Select(c => CardToVm(c, "hand")))
            .ToList();

    public IEnumerable<(int x, int y)> BoardDropZonePositions =>
        _boardService.Cards.Count == 0 ? 
            [(0, 0)] 
            : _boardService.Cards.Keys.Concat(_boardService.GetCardAdjacentPositions()).Distinct();
    
    public bool IsCardAtPosition((int x, int y) pos) =>
        _boardService.Cards.ContainsKey(pos);

    public bool CanCardBeMovedToBoardPos(CardVM card, (int x, int y) destPos)
    {
        if (card.Place == "hand")
            return true;
        
        var cardPos = GetBoardPosition(card.Place);
        // a single card should not be able to be moved adjacent to itself
        // even if this wouldn't technically break the board
        if (_boardService.Cards.Count == 1)
            return false;
        
        return _boardService.WouldCardBreakBoardIfMoved(cardPos, destPos);
    }
    
    public bool CanCardBeMovedToHand(CardVM card)
    {
        if (card.Place == "hand")
            return true;
        
        var cardPos = GetBoardPosition(card.Place);
        // a single card can move to hand
        if (_boardService.Cards.Count == 1)
            return true;
        return _boardService.WouldCardBreakBoardIfGone(cardPos);
    }

    private static CardVM CardToVm(Card card, string place) =>
        new()
        {
            Text = card.Letter.ToString(),
            Place = place,
            Id = card.Id,
        };

    public void MoveCard(CardVM cardVm, string moveToPlace)
    {
        // remove card from old place
        Card removedCard;
        if (cardVm.Place == "hand")
            removedCard = _handService.Remove(cardVm.Id);
        else
            removedCard = _boardService.Remove(GetBoardPosition(cardVm.Place));

        // add card to new place
        if (moveToPlace != "hand")
        {
            var boardPos = GetBoardPosition(moveToPlace);
            _boardService.Add(removedCard, boardPos);
        }
        else
            _handService.Add(removedCard);
    }
    
    private static (int x, int y) GetBoardPosition(string place)
    {
        var match = Regex.Match(place, @"\((-?\d+),(-?\d+)\)");
        var pos = (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
        return pos;
    }
}

public class CardVM
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Text { get; set; }
    public string Place { get; set; }
}