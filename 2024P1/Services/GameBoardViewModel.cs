using System.Text.RegularExpressions;
using _2024P1.Game;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using P1.Models;

namespace P1.Services;

public interface IGameBoardViewModel
{
    int TurnPoints { get; set; }
    TurnState TurnState { get; set; }
    
    IEnumerable<(int x, int y)> BoardDropZonePositions { get; }
    List<CardVM> Cards { get; }

    int NumCardsOnBoard { get; }
    int Turn { get; set; }
    bool IsCardAtPosition((int x, int y) pos);
    bool CanCardBeMovedToBoardPos(CardVM card, (int x, int y) destPos);
    bool CanCardBeMovedToHand(CardVM card);
    
    void MoveCard(CardVM cardVm, CardPlace moveToPlace);
    Task OnPlayCards();

    void OnBackFromResultsView();
    void OnNextTurn();
}


public class GameBoardViewModel : IGameBoardViewModel
{
    private IBoardService _boardService;
    private IHandService _handService;
    private IPlayCardsService _playCardsService;

    public int Turn { get; set; } = 0;
    public int TurnPoints { get; set; } = 0;
    public TurnState TurnState { get; set; } = TurnState.PLAYING;
    
    public GameBoardViewModel(
        IBoardService boardService, 
        IHandService handService,
        IPlayCardsService playCardsService)
    {
        _boardService = boardService;
        _handService = handService;
        _playCardsService = playCardsService;

        // initialize cardVMs if model already has state
        foreach (Card card in handService.Cards)
            _cardVmDict.Add(card.Id, new CardVM(card, new CardPlace(true, null)));
        
        foreach (var kv in boardService.BoardState)
            _cardVmDict.Add(kv.Value.Id, 
                new CardVM(kv.Value, new CardPlace(false, kv.Key)));
    }
    

    private Dictionary<int, CardVM> _cardVmDict = new();
    public List<CardVM> Cards => _cardVmDict.Values.ToList();

    public int NumCardsOnBoard =>
        _boardService.BoardState.Count;
    

    public IEnumerable<(int x, int y)> BoardDropZonePositions => _boardService.GetDropZonePositions();
    
    public bool IsCardAtPosition((int x, int y) pos) =>
        _boardService.BoardState.ContainsKey(pos);

    public bool CanCardBeMovedToBoardPos(CardVM card, (int x, int y) destPos)
    {
        if (card.Place.IsHand)
            return true;
        if (TurnState != TurnState.PLAYING)
            return false;
        
        // a single card should not be able to be moved adjacent to itself
        // even if this wouldn't technically break the board
        if (_boardService.BoardState.Count == 1)
            return false;
        
        return !_boardService.WouldCardBreakBoardIfMoved(card.Place.BoardPos.Value, destPos);
    }
    
    public bool CanCardBeMovedToHand(CardVM card)
    {
        
        if (card.Place.IsHand)
            return true;
        if (TurnState != TurnState.PLAYING)
            return false;
        
        var cardPos = card.Place.BoardPos.Value;
        // a single card can always be moved to hand
        if (_boardService.BoardState.Count == 1)
            return true;
        return !_boardService.WouldCardBreakBoardIfGone(cardPos);
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
    
    public async Task OnPlayCards()
    {
        var playCardsResult = await _playCardsService.GetPlayCardsResult(_boardService.BoardState);
        foreach (var cardLineResult in playCardsResult.CardLineResults)
            foreach (var id in cardLineResult.CardIds)
            {
                _cardVmDict[id].Highlight = cardLineResult.IsValid 
                    ? CardHighlight.Success 
                    : CardHighlight.Failure;

                _cardVmDict[id].HoverText = cardLineResult.Definition;
            }


        if (playCardsResult.IsTurnValid)
        {
            TurnPoints += playCardsResult.PointsTotal;
            TurnState = TurnState.REVIEWING_RESULTS_VALID;
        }
        else
        {
            TurnState = TurnState.REVIEWING_RESULTS_INVALID;
        }
    }

    public void OnBackFromResultsView()
    {
        ResetCardViews();
        TurnState = TurnState.PLAYING;
    }

    public void OnNextTurn()
    {
        ResetCardViews();
        TurnState = TurnState.PLAYING;
        Turn++;
    }

    private void ResetCardViews()
    {
        foreach (var card in Cards)
        {
            card.Highlight = CardHighlight.None;
            card.HoverText = null;
        }
    }
}

public enum TurnState
{
    PLAYING,
    REVIEWING_RESULTS_VALID,
    REVIEWING_RESULTS_INVALID
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

            Superscript = letterCard.Points.ToString();
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
    public string? Superscript { get; set; }
    public CardPlace Place { get; set; }
    public CardHighlight Highlight { get; set; } = CardHighlight.None;
    public string? HoverText { get; set; }
}

public class CardPlace
{
    
    public CardPlace() {}
    
    public CardPlace(bool isHand, (int x, int y)? boardPos)
    {
        IsHand = isHand;
        BoardPos = boardPos;
    }
    
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
    
    public bool IsHand { get; set; }
    public (int x, int y)? BoardPos { get; set; }

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


