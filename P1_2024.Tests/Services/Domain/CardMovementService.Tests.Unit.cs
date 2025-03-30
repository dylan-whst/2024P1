using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using P1.Models;
using P1.Services;
using System.Collections.Generic;
using Assert = Xunit.Assert;

namespace BlazorTestProject1.Services.Domain;

public class CardMovementService_Tests_Unit
{
    private IDeckService _deckServiceMock;
    private IHandService _handServiceMock;
    private IBoardService _boardServiceMock;
    private ICardMovementService _cardMovementService;

    [SetUp]
    public void SetUp()
    {
        _deckServiceMock = Substitute.For<IDeckService>();
        _handServiceMock = Substitute.For<IHandService>();
        _boardServiceMock = Substitute.For<IBoardService>();
        
        _cardMovementService = new CardMovementService(_handServiceMock, _boardServiceMock, _deckServiceMock);
    }

    [Test]
    public void MoveCard__FromHandToBoard__ShouldRemoveFromHandAndAddToBoard()
    {
        // Arrange
        var cardVm = new CardVM { Id = 1, Place = new CardPlace { IsHand = true } };
        var moveToPlace = new CardPlace { IsHand = false, BoardPos = (1, 2) };
        var card = new Card { Id = 1 };
        
        _handServiceMock.Remove(1).Returns(card);

        // Act
        _cardMovementService.MoveCard(cardVm, moveToPlace);

        // Assert
        _handServiceMock.Received(1).Remove(1);
        _boardServiceMock.Received(1).Add(card, (1, 2));
    }

    [Test]
    public void MoveCard__FromBoardToHand__ShouldRemoveFromBoardAndAddToHand()
    {
        // Arrange
        var cardVm = new CardVM { Id = 2, Place = new CardPlace { IsHand = false, BoardPos = (1, 2) } };
        var moveToPlace = new CardPlace { IsHand = true };
        var card = new Card { Id = 2 };

        _boardServiceMock.Remove((1, 2)).Returns(card);

        // Act
        _cardMovementService.MoveCard(cardVm, moveToPlace);

        // Assert
        _boardServiceMock.Received(1).Remove((1, 2));
        _handServiceMock.Received(1).Add(card);
    }

    [Test]
    public void DrawCards__ShouldDrawUntilHandIsFull()
    {
        // Arrange
        var card1 = new Card { Id = 1 };
        var card2 = new Card { Id = 2 };
        var card3 = new Card { Id = 3 };
        var drawnCards = new List<Card> { card1, card2, card3 };
        
        _handServiceMock.HandSize.Returns(3);
        _handServiceMock.Cards.Returns(new List<Card>());
        
        _deckServiceMock.Pop().Returns(card1, card2, card3);

        // Act
        var result = _cardMovementService.DrawCards();

        // Assert
        _deckServiceMock.Received(3).Pop();
        _handServiceMock.Received(3).Add(Arg.Any<Card>());
        result.Should().BeEquivalentTo(drawnCards);
    }
}
