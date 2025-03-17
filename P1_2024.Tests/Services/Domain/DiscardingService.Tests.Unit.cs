using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using P1.Models;
using P1.Services;
using Assert = Xunit.Assert;

namespace BlazorTestProject1.Services.Domain;

public class DiscardingService_Tests_Unit
{
    private IDiscardSelectionService _discardSelectionService;
    private ITurnService _turnServiceMock;
    private IHandService _handServiceMock;

    [SetUp]
    public void SetUp()
    {
        _turnServiceMock = Substitute.For<ITurnService>();
        _handServiceMock = Substitute.For<IHandService>();
        _discardSelectionService = 
            new DiscardSelectionService(
                    _turnServiceMock, 
                    _handServiceMock)
                .SetMaxDiscardSelection(2);
    }

    [Test]
    public void OnStartDiscarding__ShouldSetTurnStateToDiscarding()
    {
        // Act
        _discardSelectionService.OnStartDiscarding();

        // Assert
        _turnServiceMock.Received(1).TurnState = TurnState.DISCARDING;
    }

    [Test]
    public void OnCancelDiscarding__ShouldSetTurnStateToPlaying()
    {
        // Act
        _discardSelectionService.OnStopDiscarding();

        // Assert
        _turnServiceMock.Received(1).TurnState = TurnState.PLAYING;
    }
    
    [Test]
    public void OnCancelDiscarding__WithCardsSelected__ShouldResetSelected()
    {
        // Arrange
        _discardSelectionService.ToggleSelectForDiscard(new CardVM());
        _discardSelectionService.ToggleSelectForDiscard(new CardVM());
        
        // Act
        _discardSelectionService.OnStopDiscarding();
        
        // Assert
        _discardSelectionService.SelectedForDiscard.Count.Should().Be(0);
    }

    [Test]
    public void OnToggleSelectForDiscard__ShouldAddCardToSelectedForDiscard()
    {
        // Arrange
        var card = new CardVM();

        // Act
        _discardSelectionService.ToggleSelectForDiscard(card);

        // Assert
        _discardSelectionService.SelectedForDiscard.Should().ContainSingle().Which.Should().Be(card);
    }
    
    [Test]
    public void OnToggleSelectForDiscard__AlreadySelected__ShouldRemoveSelectedForDiscard()
    {
        // Arrange
        var card = new CardVM();
        _discardSelectionService.ToggleSelectForDiscard(card);

        // Act
        _discardSelectionService.ToggleSelectForDiscard(card);

        // Assert
        _discardSelectionService.SelectedForDiscard.Should().BeEmpty();
    }
    
    [Test]
    public void OnToggleSelectForDiscard__DiscardLimitHit__NoSelectionChange()
    {
        // Arrange
        _discardSelectionService.MaxDiscardSelection.Should().Be(2); // configured in setup
        var card1 = new CardVM();
        var card2 = new CardVM();
        var card3 = new CardVM();
        _discardSelectionService.ToggleSelectForDiscard(card1);
        _discardSelectionService.ToggleSelectForDiscard(card2);
        
        // Act
        _discardSelectionService.ToggleSelectForDiscard(card3);

        // Assert
        _discardSelectionService.SelectedForDiscard.Should().Contain([card1, card2]);
    }

    [Test]
    public void OnToggleSelectForDiscard__ShouldTriggerOnDiscardStateChangedEvent()
    {
        var card = new CardVM();
        bool eventTriggered = false;
        _discardSelectionService.OnDiscardStateChanged += () => eventTriggered = true;
        
        _discardSelectionService.ToggleSelectForDiscard(card);
        
        eventTriggered.Should().BeTrue();
    }
    
    [Test]
    public void DiscardSelectedCards__WithCardsSelected__RemovesThemFromHand()
    {
        // Arrange
        _discardSelectionService.ToggleSelectForDiscard(new CardVM() {Id = 1});
        _discardSelectionService.ToggleSelectForDiscard(new CardVM() {Id = 2});
        
        // Act
        _discardSelectionService.DiscardSelectedCards();
        
        // Assert
        _handServiceMock.Received(1).Remove(1);
        _handServiceMock.Received(1).Remove(2);
    }
}