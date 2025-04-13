using Microsoft.AspNetCore.Components.Web;
using P1.Models;

namespace P1.Services;

/// <summary>
/// This is the root object that glues the domain layer to view components.
/// 1. Handles view-specific state such as hovered cards and highlighting.
/// 2. Internally relays and orchestrates business objects to create a facade for views.
/// </summary>
public interface IGameBoardViewModel
{
    // view state
    List<CardVM> Cards { get; }
    event Action OnViewStateChanged;
    CardVM? CardMousedOver { get; set; }

    
    // turn view
    int TurnPoints { get; }
    TurnState TurnState { get; }
    int Turn { get; }
    
    // board view
    int BoardSize { get; }
    (int x, int y) BoardCenter { get; }
    IEnumerable<(int x, int y)> BoardDropZonePositions { get; }
    int NumCardsOnBoard { get; }
    bool IsBoardValid { get; }
    bool IsCardAtPosition((int x, int y) pos);
    
    // User Actions
    void OnMoveCard(CardVM cardVm, CardPlace moveToPlace);
    Task OnPlayCards();
    void OnBackFromResultsView();
    void OnNextTurn();
    void OnDiscardSelectedCards();
}


public class GameBoardViewModel : IGameBoardViewModel
{
    private IBoardService _boardService;
    private IHandService _handService;
    private IPlayCardsService _playCardsService;
    private ITurnService _turnService;
    private ICardMovementService _cardMovementService;
    private IDiscardSelectionService _discardSelectionService;
    
    public GameBoardViewModel(
        IBoardService boardService, 
        IHandService handService,
        IPlayCardsService playCardsService,
        ITurnService turnService,
        ICardMovementService cardMovementService,
        IDiscardSelectionService discardSelectionService)
    {
        _boardService = boardService;
        _handService = handService;
        _playCardsService = playCardsService;
        _turnService = turnService;
        _cardMovementService = cardMovementService;
        _discardSelectionService = discardSelectionService;

        // initialize cardVMs if model already has state
        foreach (Card card in handService.Cards)
            _cardVmDict.Add(card.Id, new CardVM(card, new CardPlace(true, null)));
        
        foreach (var kv in boardService.BoardState)
            _cardVmDict.Add(kv.Value.Id, 
                new CardVM(kv.Value, new CardPlace(false, kv.Key)));
        
        DrawCards();
    }
    
    // view state
    private Dictionary<int, CardVM> _cardVmDict = new();
    public List<CardVM> Cards => _cardVmDict.Values.ToList();
    public event Action? OnViewStateChanged;
    private CardVM? _cardMousedOver;
    public CardVM? CardMousedOver
    {
        get => _cardMousedOver;
        set
        {
            if (_cardMousedOver != value)
            {
                _cardMousedOver = value;
                OnViewStateChanged.Invoke();
            }
        }
    }

    
    // view facade for turn service
    public int Turn => _turnService.Turn;
    public int TurnPoints => _turnService.TurnPoints;
    public TurnState TurnState => _turnService.TurnState;

    // view facade for board service
    public int NumCardsOnBoard =>
        _boardService.BoardState.Count;
    
    public bool IsBoardValid {
        get
        {
            if (NumCardsOnBoard == 0)
                return false;
            
            // var newlyPlaced = Cards.Where(c => c.IsCemented == false && c.Place.IsHand == false);
            // var isXInOneLine = newlyPlaced.Select(c => c.Place.BoardPos?.x).Distinct().Count() == 1;
            // var isYInOneLine = newlyPlaced.Select(c => c.Place.BoardPos?.y).Distinct().Count() == 1;
            // if(!(isXInOneLine || isYInOneLine))
            //     return false;
            
            return true;
        }
    }


    public IEnumerable<(int x, int y)> BoardDropZonePositions => _boardService.GetDropZonePositions();
    public int BoardSize => _boardService.BoardSize;
    public (int x, int y) BoardCenter => _boardService.BoardCenter;
    public bool IsCardAtPosition((int x, int y) pos) =>
        _boardService.BoardState.ContainsKey(pos);
    
    
    public void OnMoveCard(CardVM cardVm, CardPlace moveToPlace)
    {
        _cardMovementService.MoveCard( cardVm, moveToPlace);

        // synchronize change with view model
        _cardVmDict[cardVm.Id].Place = moveToPlace;
        OnViewStateChanged.Invoke();
    }
    
    public async Task OnPlayCards()
    {
        var playCardsResult = await _playCardsService.GetPlayCardsResult(_boardService.BoardState);
        foreach (var cardLineResult in playCardsResult.CardLineResults)
            foreach (var id in cardLineResult.CardIds)
            {
                if (cardLineResult is { IsValid: true, IsAlreadyPlayed: false })
                    _cardVmDict[id].Highlight = CardHighlight.Success;
                else if (cardLineResult is { IsValid: false, IsAlreadyPlayed: false })
                    _cardVmDict[id].Highlight = CardHighlight.Failure;

                _cardVmDict[id].HoverText = cardLineResult.Definition;
            }

        if (playCardsResult.IsTurnValid)
        {
            _turnService.SetToReviewing(isValid: true);
            
            _turnService.TurnPoints += playCardsResult.PointsTotal;
            foreach (var id in playCardsResult.CardLineResults.SelectMany(cardLineResult => cardLineResult.CardIds))
                _cardVmDict[id].IsCemented = true;
        }
        else
        {
            _turnService.SetToReviewing(isValid: false);
        }

        OnViewStateChanged.Invoke();
    }

    public void OnBackFromResultsView()
    {
        ResetCardViews();
        _turnService.TurnState = TurnState.PLAYING;
        
        OnViewStateChanged.Invoke();
    }

    public void OnNextTurn()
    {
        ResetCardViews();
        DrawCards();
        
        _turnService.ProgressTurn();
        
        OnViewStateChanged.Invoke();
    }

    public void OnDiscardSelectedCards()
    {
        var discardedCards = new List<CardVM>(_discardSelectionService.SelectedForDiscard);
        _discardSelectionService.DiscardSelectedCards();
        
        foreach (var cardVm in discardedCards)
            _cardVmDict.Remove(cardVm.Id);
        
        _discardSelectionService.OnStopDiscarding();
        
        var drawnCards = _cardMovementService.DrawCards();
        foreach (var card in drawnCards)
            AddCardToVM(card, CardPlace.InHand);
        
        OnViewStateChanged.Invoke();
    }

    private void AddCardToVM(Card card, CardPlace place)
    {
        _cardVmDict[card.Id] = new CardVM(card, place);
    }

    private void DrawCards()
    {
        var drawnCards = _cardMovementService.DrawCards();
        foreach (var card in drawnCards)
            _cardVmDict[card.Id] = new CardVM(card, CardPlace.InHand);
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
