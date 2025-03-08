// using System.Text.RegularExpressions;
// using _2024P1.Game;
// using Microsoft.AspNetCore.Components.Web;
// using MudBlazor;
// using P1.Models;
//
// namespace P1.Services;
//
// public interface IGameRulesService
// {
//     bool CanCardBeMovedToHand(CardVM card);
//     bool CanCardBeDroppedAt(CardVM card, (int x, int y) pos);
// }
//
//
// public class GameRulesService : IGameRulesService
// {
//     private IBoardService _boardService;
//     private IHandService _handService;
//     private IPlayCardsService _playCardsService;
//     private IDeckService _deckService;
//     
//
//
//     public GameRulesService(
//         IBoardService boardService,
//         IHandService handService,
//         IDeckService deckService,
//         IPlayCardsService playCardsService)
//     {
//         _boardService = boardService;
//         _handService = handService;
//         _deckService = deckService;
//         _playCardsService = playCardsService;
//     }
//     
//
//     private bool IsCardAtPosition((int x, int y) pos) =>
//         _boardService.BoardState.ContainsKey(pos);
//
//     public bool CanCardBeDroppedAt(CardVM card, (int x, int y) pos)
//     {
//         return !IsCardAtPosition(pos)
//                && CanCardBeMovedToBoardPos(card, pos);
//     }
//
//     public bool CanCardBeMovedToBoardPos(CardVM card, (int x, int y) destPos)
//     {
//         if (card.Place.IsHand)
//             return true;
//         if (TurnState != TurnState.PLAYING)
//             return false;
//         if (card.IsCemented)
//             return false;
//
//         // a single card should not be able to be moved adjacent to itself
//         // even if this wouldn't technically break the board
//         if (_boardService.BoardState.Count == 1)
//             return false;
//
//         return !_boardService.WouldCardBreakBoardIfMoved(card.Place.BoardPos.Value, destPos);
//     }
//
//     public bool CanCardBeMovedToHand(CardVM card)
//     {
//
//         if (card.Place.IsHand)
//             return true;
//         if (TurnState != TurnState.PLAYING)
//             return false;
//         if (card.IsCemented)
//             return false;
//
//         var cardPos = card.Place.BoardPos.Value;
//         // a single card can always be moved to hand
//         if (_boardService.BoardState.Count == 1)
//             return true;
//         return !_boardService.WouldCardBreakBoardIfGone(cardPos);
//     }
//     
// }