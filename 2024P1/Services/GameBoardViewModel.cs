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
    void MoveCard(CardVM cardVm, string place);
}


public class GameBoardViewModel : IGameBoardViewModel
{
    private IBoard _board;
    private IHand _hand;
    
    public GameBoardViewModel(IBoard board, IHand hand)
    {
        _board = board;
        _hand = hand;
    }

    public IEnumerable<(int x, int y)> BoardDropZonePositions =>
        _board.Cards.Count == 0 ? 
            [(0, 0)] 
            : _board.Cards.Keys.Concat(_board.GetCardAdjacentPositions()).Distinct();

    public List<CardVM> Cards =>
            _board.Cards.Select(c => cardToVm(c.Value, $"board-({c.Key.x},{c.Key.y})"))
                .Concat(_hand.Cards.Select(c => cardToVm(c, "hand")))
                .ToList();

    private CardVM cardToVm(Card card, string place) =>
        new CardVM()
        {
            Text = card.Letter.ToString(),
            Place = place,
            Id = card.Id,
            Name = "Card"
        };

    public void MoveCard(CardVM cardVm, string place)
    {
        if (cardVm.Place == "hand")
            _hand.Remove(cardVm.Id);
        else
            _board.Remove(GetBoardPosition(cardVm.Place));
        
        var boardPos = GetBoardPosition(place);
        _board.Add(
            new Card() {
                Id = cardVm.Id,
                Letter = char.Parse(cardVm.Text) }, 
            boardPos);
        
        cardVm.Place = place;
    }
    
    public bool IsCardAtPosition((int x, int y) pos)
    {
        return _board.Cards.ContainsKey(pos);
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