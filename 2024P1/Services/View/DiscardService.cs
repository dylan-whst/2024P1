using Microsoft.AspNetCore.Components.Web;
using P1.Models;

namespace P1.Services;

/// <summary>
/// Used by views for managing the discarding of cards.
/// </summary>
public interface IDiscardSelectionService
{
    List<CardVM> SelectedForDiscard { get; }

    /// Represents the maximum number of cards that can be selected for discarding.
    /// This property determines the limit on the number of cards a player can choose to discard during the discard phase.
    int MaxDiscardSelection { get; }

    void DiscardSelectedCards();
    void OnStartDiscarding();
    void OnStopDiscarding();

    /// <summary>
    /// Toggles the selection state of a card for discarding. If the maximum discard selection limit has been reached, the card won't be added to or removed from the selection.
    /// </summary>
    /// <param name="card">The card to toggle the selection state for.</param>
    void ToggleSelectForDiscard(CardVM card);
    
    
    event Action? OnDiscardStateChanged;
}

public class DiscardSelectionService : IDiscardSelectionService
{
    private ITurnService _turnService;
    private IHandService _handService;

    public DiscardSelectionService(ITurnService turnService, IHandService handService)
    {
        _turnService = turnService;
        _handService = handService;
    }
    
    public IDiscardSelectionService SetMaxDiscardSelection(int n)
    {
        MaxDiscardSelection = n;
        return this;
    }

    public List<CardVM> SelectedForDiscard { get; private set; } = new List<CardVM>();
    public int MaxDiscardSelection { get; private set; } = 2;

    public void DiscardSelectedCards()
    {
        foreach (var card in SelectedForDiscard)
            _handService.Remove(card.Id);
    }

    public void OnStartDiscarding()
    {
        _turnService.TurnState = TurnState.DISCARDING;
    }

    public void OnStopDiscarding()
    {
        _turnService.TurnState = TurnState.PLAYING;
        SelectedForDiscard.Clear();
        OnDiscardStateChanged.Invoke();
    }

    public void ToggleSelectForDiscard(CardVM card)
    {
        // toggle selection
        if (SelectedForDiscard.Contains(card))
            SelectedForDiscard.Remove(card);
        else if (SelectedForDiscard.Count < MaxDiscardSelection)
            SelectedForDiscard.Add(card);
        
        OnDiscardStateChanged?.Invoke();
    }

    public event Action? OnDiscardStateChanged;
}