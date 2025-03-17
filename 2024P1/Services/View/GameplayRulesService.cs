using System.Text.RegularExpressions;
using _2024P1.Game;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using P1.Models;

namespace P1.Services;

public interface IGameplayRulesService
{
    bool CanCardBeMovedToHand(CardVM card);
    bool CanCardBeDroppedAt(CardVM card, (int x, int y) pos);
}


public class GameplayRulesService : IGameplayRulesService
{
    private IBoardService _boardService;
    private ITurnService _turnService;

    public GameplayRulesService(
        IBoardService boardService,
        ITurnService turnService)
    {
        _boardService = boardService;
        _turnService = turnService;
    }
    

    private bool IsCardAtPosition((int x, int y) pos) =>
        _boardService.BoardState.ContainsKey(pos);

    public bool CanCardBeDroppedAt(CardVM card, (int x, int y) pos)
    {
        return !IsCardAtPosition(pos)
               && CanCardBeMovedToBoardPos(card, pos);
    }

    private bool CanCardBeMovedToBoardPos(CardVM card, (int x, int y) destPos)
    {
        if (card.Place.IsHand)
            return true;
        if (_turnService.TurnState != TurnState.PLAYING)
            return false;
        if (card.IsCemented)
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
        if (_turnService.TurnState != TurnState.PLAYING)
            return false;
        if (card.IsCemented)
            return false;

        var cardPos = card.Place.BoardPos.Value;
        // a single card can always be moved to hand
        if (_boardService.BoardState.Count == 1)
            return true;
        return !_boardService.WouldCardBreakBoardIfGone(cardPos);
    }
}