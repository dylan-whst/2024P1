
using P1.Models;

namespace P1.Services;

public interface ICardMovementService
{
    void MoveCard(CardVM cardVm, CardPlace moveToPlace);
    
    /// <returns> A list of the cards that were drawn </returns>
    List<Card> DrawCards();
    List<Card> DrawCards(int numCards);
}


public class CardMovementService : ICardMovementService
{
    private IDeckService _deckService;
    private IHandService _handService;
    private IBoardService _boardService;

    public CardMovementService(IHandService handService, IBoardService boardService, IDeckService deckService)
    {
        _handService = handService;
        _boardService = boardService;
        _deckService = deckService;
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
    }

    public List<Card> DrawCards()
    {
        var drawnCards = new List<Card>();
        while (_handService.Cards.Count < _handService.HandSize)
        {
            // move card from deck to hand and vm
            var card = _deckService.Pop();
            _handService.Add(card);
            drawnCards.Add(card);
        }
        
        return drawnCards;
    }

    public List<Card> DrawCards(int numCards)
    {
        var drawnCards = new List<Card>();
        for (int i = 0; i < numCards; i++)
        {
            // move card from deck to hand and vm
            var card = _deckService.Pop();
            _handService.Add(card);
            drawnCards.Add(card);
        }
        
        return drawnCards;
    }
}