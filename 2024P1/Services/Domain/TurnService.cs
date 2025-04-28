using System.Text.RegularExpressions;
using _2024P1.Game;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using P1.Models;

namespace P1.Services;

public interface ITurnService
{
    int TurnPoints { get; set; }
    TurnState TurnState { get; set; }
    int Turn { get; }
    
    int StartingDiscards { get; }
    int NumDiscardsLeft { get; set; }

    void ProgressTurn();
    void SetToReviewing(bool isValid);
}


public class TurnService : ITurnService
{
    public TurnService()
    {
    }
    

    public int Turn { get; set; } = 0;
    public int TurnPoints { get; set; } = 0;
    public TurnState TurnState { get; set; } = TurnState.PLAYING;
    

    public int StartingDiscards { get; } = 2;
    public int NumDiscardsLeft { get; set; } = 2;

    public int PointsGoal { get; } = 20;
    
    public void ProgressTurn()
    {
        Turn++;
        TurnState = TurnState.PLAYING;
    }

    public void SetToReviewing(bool isValid)
    {
        if (isValid)
            TurnState = TurnState.REVIEWING_RESULTS_VALID;
        else
            TurnState = TurnState.REVIEWING_RESULTS_INVALID;
    }
}