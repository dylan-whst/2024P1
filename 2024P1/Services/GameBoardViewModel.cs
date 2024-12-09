using System.Text.RegularExpressions;
using _2024P1.Game;
using MudBlazor;
using P1.Models;

namespace P1.Services;

public interface IGameBoardViewModel
{
    IEnumerable<(int x, int y)> BoardDropZonePositions { get; }
    List<CardVM> Cards { get; }

    int NumCardsOnBoard { get; }
    bool IsCardAtPosition((int x, int y) pos);
    bool CanCardBeMovedToBoardPos(CardVM card, (int x, int y) destPos);
    bool CanCardBeMovedToHand(CardVM card);
    
    void MoveCard(CardVM cardVm, CardPlace moveToPlace);
    void PlayCards();

}


public class GameBoardViewModel : IGameBoardViewModel
{
    private IBoardService _boardService;
    private IHandService _handService;
    private IWordValidator _wordValidator;
    
    public GameBoardViewModel(
        IBoardService boardService, 
        IHandService handService,
        IWordValidator wordValidator)
    {
        _boardService = boardService;
        _handService = handService;
        _wordValidator = wordValidator;

        foreach (Card card in handService.Cards)
            _cardVmDict.Add(card.Id, new CardVM(card, new CardPlace() { IsHand = true }));
    }

    private Dictionary<int, CardVM> _cardVmDict = new();
    public List<CardVM> Cards => _cardVmDict.Values.ToList();

    public int NumCardsOnBoard =>
        _boardService.Cards.Count();

    public IEnumerable<(int x, int y)> BoardDropZonePositions =>
        _boardService.Cards.Count == 0 ? 
            [(0, 0)] 
            : _boardService.Cards.Keys.Concat(_boardService.GetCardAdjacentPositions()).Distinct();
    
    public bool IsCardAtPosition((int x, int y) pos) =>
        _boardService.Cards.ContainsKey(pos);

    public bool CanCardBeMovedToBoardPos(CardVM card, (int x, int y) destPos)
    {
        if (card.Place.IsHand)
            return true;
        
        // a single card should not be able to be moved adjacent to itself
        // even if this wouldn't technically break the board
        if (_boardService.Cards.Count == 1)
            return false;
        
        return _boardService.WouldCardBreakBoardIfMoved(card.Place.BoardPos.Value, destPos);
    }
    
    public bool CanCardBeMovedToHand(CardVM card)
    {
        if (card.Place.IsHand)
            return true;
        
        var cardPos = card.Place.BoardPos.Value;
        // a single card can move to hand
        if (_boardService.Cards.Count == 1)
            return true;
        return _boardService.WouldCardBreakBoardIfGone(cardPos);
    }

    public void MoveCard(CardVM cardVm, CardPlace moveToPlace)
    {
        // remove card from old place
        Card removedCard;
        if (cardVm.Place.IsHand)
            removedCard = _handService.Remove(cardVm.Id);
        else
            removedCard = _boardService.Remove(cardVm.Place.BoardPos.Value);

        // add card to new place
        if (!moveToPlace.IsHand)
        {
            _boardService.Add(removedCard, moveToPlace.BoardPos.Value);
        }
        else
            _handService.Add(removedCard);

        // synchronize change that happened with the board with the model
        _cardVmDict[cardVm.Id].Place = moveToPlace;
    }
    
    public void PlayCards()
    {
        foreach (var card in Cards.Where(c => !c.Place.IsHand))
        {
            card.Highlight = CardHighlight.Success;
        }
    }

}

public class CardVM
{

    public CardVM()
    {
        
    }
    public CardVM(Card card, CardPlace place)
    {
        if (card is LetterCard letterCard)
        {

            Text = letterCard.Letter.ToString();
            Place = place;
            Id = letterCard.Id;
        }
        else
        {
            throw new NotSupportedException($"could not create view for card '{card.Id}': it is of an unsupported type");
        }
    }
    
    public int Id { get; set; }
    public string Name { get; set; }
    public string Text { get; set; }
    public CardPlace Place { get; set; }
    public CardHighlight Highlight { get; set; } = CardHighlight.None;
}

public class CardPlace
{
    public bool IsHand { get; set; }
    public (int x, int y)? BoardPos { get; set; }
    
    public CardPlace() {}

    public CardPlace(string dropzoneRep)
    {
        if (dropzoneRep == "hand")
            IsHand = true;
        else
        {
            IsHand = false;
            var match = Regex.Match(dropzoneRep, @"^board-\((-?\d+),(-?\d+)\)$");
            var pos = (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
            BoardPos = pos;
        }
    }

    public override string ToString()
    {
        if (IsHand)
            return "hand";
        else
            return $"board-({BoardPos.Value.x},{BoardPos.Value.y})";
    }

    public static CardPlace InHand => new CardPlace() { IsHand = true };
}

public enum CardHighlight
{
    Success,
    Failure,
    None
}
