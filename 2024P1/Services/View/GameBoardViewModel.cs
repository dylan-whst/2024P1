using _2024P1.Game.Shop;
using Microsoft.AspNetCore.Components.Web;
using P1.Models;
using P1.Models.Shop;

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
    List<CardVM> HandCards { get; }
    event Action OnViewStateChanged;
    CardVM? CardMousedOver { get; set; }
    double CardSizePx { get; }
    bool IsFirstVisit { get; set; }

    
    // turn view
    int TurnPoints { get; }
    TurnState TurnState { get; }
    int HandsLeft { get; }
    
    // board view
    int BoardSize { get; }
    (int x, int y) BoardCenter { get; }
    IEnumerable<(int x, int y)> BoardDropZonePositions { get; }
    int NumCardsOnBoard { get; }
    bool IsBoardValid { get; }
    bool IsDraggingDisabled(CardVM card);
    bool IsCardAtPosition((int x, int y) pos);
    
    // User Actions
    void OnMoveCard(CardVM cardVm, CardPlace moveToPlace);
    Task OnPlayCards();
    void OnBackFromResultsView();
    void OnNextTurn();
    void OnDiscardSelectedCards();
    void ShuffleHand();
    void ReturnHand(MouseEventArgs obj);
    void Upgrade(Upgrade upgrade);
    void Reset();
}


public class GameBoardViewModel : IGameBoardViewModel
{
    private IBoardService _boardService;
    private IHandService _handService;
    private IPlayCardsService _playCardsService;
    private ITurnService _turnService;
    private ICardMovementService _cardMovementService;
    private IDiscardSelectionService _discardSelectionService;
    private IPlayerAttributesService _playerAttributesService;
    private IDeckService _deckService;
    
    public GameBoardViewModel(
        IBoardService boardService, 
        IHandService handService,
        IPlayCardsService playCardsService,
        ITurnService turnService,
        ICardMovementService cardMovementService,
        IDiscardSelectionService discardSelectionService,
        IPlayerAttributesService playerAttributesService,
        IDeckService deckService)
    {
        _boardService = boardService;
        _handService = handService;
        _playCardsService = playCardsService;
        _turnService = turnService;
        _cardMovementService = cardMovementService;
        _discardSelectionService = discardSelectionService;
        _playerAttributesService = playerAttributesService;
        _deckService = deckService;

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
    public List<CardVM> HandCards => _cardVmDict.Values.Where(c => c.Place.IsHand).ToList();
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
    public bool IsFirstVisit { get; set; } = true;


    
    // view facade for turn service
    public int HandsLeft => _turnService.NumHandsLeft;
    public double CardSizePx =>  ((double) 5/BoardSize) * 85;
    public int TurnPoints => _turnService.Points;
    public TurnState TurnState => _turnService.TurnState;

    // view facade for board service
    public int NumCardsOnBoard =>
        _boardService.BoardState.Count;
    
    public bool IsBoardValid {
        get
        {
            if (NumCardsOnBoard == 0)
                return false;

            // invalid if no new cards on board
            if (Cards.Where(c => !c.Place.IsHand).All(c => c.IsCemented))
                return false;
            
            // enforce single line of cards:
            
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
    public bool IsDraggingDisabled(CardVM card)
    {
        if (_turnService.TurnState != TurnState.PLAYING)
            return true;

        return false;
    }

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
            
            _turnService.Points += playCardsResult.PointsTotal;
            foreach (var id in playCardsResult.CardLineResults.SelectMany(cardLineResult => cardLineResult.CardIds))
                _cardVmDict[id].IsCemented = true;
            
            _playCardsService.RememberPlayedWords(playCardsResult.CardLineResults.Select(res => res.CardIds).ToList());
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
        
        _turnService.NumDiscardsLeft -= 1;
        _discardSelectionService.OnStopDiscarding();
        
        var drawnCards = _cardMovementService.DrawCards(discardedCards.Count());
        foreach (var card in drawnCards)
            AddCardToVM(card, CardPlace.InHand);
        
        OnViewStateChanged.Invoke(); }

    public void ShuffleHand()
    {
        var rng = new Random();
        foreach (var card in HandCards)
        {
            _cardVmDict[card.Id].HandOrder = rng.Next();
        }
        OnViewStateChanged.Invoke();
    }

    public void ReturnHand(MouseEventArgs obj)
    {
        foreach (var cardVm in Cards.Where(c => !c.IsCemented && !c.Place.IsHand))
        {
            var card = _boardService.Remove(cardVm.Place.BoardPos.Value);
            _handService.Add(card);
            _cardVmDict[card.Id].Place = CardPlace.InHand;
        }
        
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
        {
            _cardVmDict[card.Id] = new CardVM(card, CardPlace.InHand);
        }
            
    }

    private void ResetCardViews()
    {
        foreach (var card in Cards)
        {
            card.Highlight = CardHighlight.None;
            card.HoverText = null;
        }
    }

    public void Upgrade(Upgrade upgrade)
    {
        if (upgrade == Models.Shop.Upgrade.PLUS_1_BOARD)
        {
            _boardService.BoardSize += 1;
        } else if (upgrade == Models.Shop.Upgrade.PLUS_2_HAND)
        {
            _handService.HandSize += 1;
        } else if (upgrade == Models.Shop.Upgrade.PLUS_2_DISCARDS)
        {
            _playerAttributesService.Discards += 2;
        }
    }

    public void Reset()
    {
        _boardService.Reset();
        _handService.Reset();
        _deckService.Reset();
        _cardVmDict = new();
        DrawCards();
    }
}
