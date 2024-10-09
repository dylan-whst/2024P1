using System.Text.RegularExpressions;
using _2024P1.Game;
using MudBlazor;
using P1.Models;

namespace P1.Services;

public interface IGameBoardViewModel
{
    List<(int x, int y)> BoardDropZonePositions { get; }
    bool IsCardAtPosition((int x, int y) pos);
    List<CardVM> Cards { get; }
    void MoveCard(CardVM cardVm, string place);
}

public class Card
{
    public char Letter { get; set; }
    public int Id;
}

public interface IBoard
{
    Dictionary<(int x, int y), Card> Cards { get; }

    void Add(Card card, (int x, int y) pos);
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
}


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


public class GameBoardViewModel : IGameBoardViewModel
{
    private IBoard _board;
    private IHand _hand;
    
    public GameBoardViewModel(IBoard board, IHand hand)
    {
        _board = board;
        _hand = hand;
    }
    
    public List<(int x, int y)> BoardDropZonePositions { get; } = [(0, 0)];

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
        
        var boardPos = GetBoardPosition(place);
        _board.Add(
            new Card() {
                Id = cardVm.Id,
                Letter = char.Parse(cardVm.Text) }, 
            boardPos);

        foreach (var adjacentPos in GetAdjacent(boardPos)
                     .Where(adjacentPos => !BoardDropZonePositions.Contains(adjacentPos)))
            BoardDropZonePositions.Add(adjacentPos);
        
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

    private static List<(int x, int y)> GetAdjacent((int x, int y) pos) => [
        pos with { y = pos.y + 1 },
        pos with { y = pos.y - 1 },
        pos with { x = pos.x + 1 },
        pos with { x = pos.x - 1 }
    ];
}