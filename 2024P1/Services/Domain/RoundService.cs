using System.Text.RegularExpressions;
using _2024P1.Game;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using P1.Models;

namespace P1.Services;

public interface IRoundsService
{
    int Round { get; }
    RoundState State { get; }

    event Action OnRoundStateChanged;
    void ProgressToShop();
    void ProgressToBoard();
}


public class RoundsService : IRoundsService
{
    private ITurnService _turnService;
    public RoundsService(ITurnService turnService)
    {
        _turnService = turnService;
    }

    public int Round { get; private set; } = 1;
    public RoundState State { get; private set; }

    public void ProgressToShop()
    {
        Round += 1;
        State = RoundState.SHOPPING;
        OnRoundStateChanged?.Invoke();
    }

    public void ProgressToBoard()
    {
        State = RoundState.PLAYING;
        OnRoundStateChanged?.Invoke();
        _turnService.Reset();
    }

    public event Action? OnRoundStateChanged;
}

public enum RoundState
{
    PLAYING,
    SHOPPING,
}