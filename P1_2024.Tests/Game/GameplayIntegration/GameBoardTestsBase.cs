namespace BlazorTestProject1.Game;

using Bunit;
using FluentAssertions;

public abstract class GameBoardTestBase : BunitTestContext
{
    protected IRenderedFragment _cut;
    protected string BoardStr(int x, int y) => $"board-({x},{y})";
    
    protected void CardShouldBeIn(int cardId, string container)
    {
        _cut.FindAll($"[identifier='{container}'] #card-{cardId}").Should().NotBeEmpty();
    }

    protected void CardShouldNotBeIn(int cardId, string container)
    {
        _cut.FindAll($"[identifier='{container}'] #card-{cardId}").Should().BeEmpty();
    }

    protected void DragCardToContainer(int cardId, string container)
    {
        _cut.Find($"#card-{cardId}").MouseOver();
        _cut.Find($"#card-{cardId}").DragStart();
        _cut.Find($"[identifier='{container}']").Drop();
    }

    protected void BoardShouldHaveDropZone(int x, int y)
    {
        _cut.Find($"[identifier='{BoardStr(x, y)}']").Should().NotBeNull();
    }

    protected void BoardShouldNotHaveCardAt(int x, int y)
    {
        _cut.FindAll($"[identifier='{BoardStr(x, y)}'] .card").Should().BeEmpty();
    }

    protected void BoardShouldHaveCardAt(int x, int y)
    {
        _cut.Find($"[identifier='{BoardStr(x, y)}'] .card");
    }
}
