using System.Text.RegularExpressions;
using _2024P1.Game;
using _2024P1.Game.Board;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using P1.Models;

namespace P1.Services;

public interface ITurnService
{
    int Points { get; set; }
    int PointsGoal { get; }
    bool IsGoalAchieved { get; }
    TurnState TurnState { get; set; }
    int NumDiscardsLeft { get; set; }
    int NumHandsLeft { get; set; }

    void ProgressTurn();
    void SetToReviewing(bool isValid);
    void Reset();
}


public class TurnService : ITurnService
{
    private IPlayerAttributesService _playerAttributes;
    private IDialogService _dialogService;
    public TurnService(IPlayerAttributesService playerAttributes, IDialogService dialogService)
    {
        _playerAttributes = playerAttributes;
        _dialogService = dialogService;
    }

    public int NumHandsLeft { get; set; } = 3;
    public int Points { get; set; } = 0;
    public TurnState TurnState { get; set; } = TurnState.PLAYING;
    
    public int NumDiscardsLeft { get; set; } = 2;

    public int PointsGoal { get; private set; } = 20;
    public bool IsGoalAchieved => Points >= PointsGoal;
    
    public void ProgressTurn()
    {
        NumHandsLeft--;
        TurnState = TurnState.PLAYING;

        if (NumHandsLeft == -1)
        {
            NumHandsLeft = 0; // dont show -1 under gameover menu
            _dialogService.Show<GameOverDialog>();
        }
    }

    public void SetToReviewing(bool isValid)
    {
        if (isValid)
            TurnState = TurnState.REVIEWING_RESULTS_VALID;
        else
            TurnState = TurnState.REVIEWING_RESULTS_INVALID;
    }

    public void Reset()
    {
        TurnState = TurnState.PLAYING;
        NumHandsLeft = _playerAttributes.Hands;
        NumDiscardsLeft = _playerAttributes.Discards;
        Points = 0;
        PointsGoal += 5;
    }
}