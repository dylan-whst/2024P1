using P1.Models;

namespace P1.Services;

public interface IPlayCardsService
{
    Task<PlayCardsResult> GetPlayCardsResult(Dictionary<(int x, int y), Card> boardCards);
}

public class PlayCardsService: IPlayCardsService
{
    private static List<List<int>> PlayedWordHistory { get; set; } = [];
    
    private readonly IWordValidator _wordValidator;

    public PlayCardsService(IWordValidator wordValidator)
    {
        _wordValidator = wordValidator;
    }

    public async Task<PlayCardsResult> GetPlayCardsResult(Dictionary<(int x, int y), Card> boardCards)
    {
        
        var yIndexes = boardCards.Select(kv => kv.Key.y).Distinct();
        var xIndexes = boardCards.Select(kv => kv.Key.x).Distinct();
        
        List<List<Card>> cardLines = new();
        foreach (var y in yIndexes)
        {
            // select all cards in a y row, ordered by their x
            var cardRow = boardCards
                .Where(kv => kv.Key.y == y)
                .OrderBy(kv => kv.Key.x);

            var rowCardLines = GetConsecutiveCardLines(cardRow, isVertical: false);
            
            foreach (var cardLine in rowCardLines)
                cardLines.Add(cardLine);
        }
        
        foreach (var x in xIndexes)
        {
            // select all cards in a x column, ordered by their y
            var cardColumn = boardCards
                .Where(kv => kv.Key.x == x)
                .OrderBy(kv => kv.Key.y);

            var colCardLines = GetConsecutiveCardLines(cardColumn, isVertical: true);

            foreach (var cardLine in colCardLines)
                cardLines.Add(cardLine);
        }

        var result = new PlayCardsResult();
        
        foreach (var cardLine in cardLines)
            result.CardLineResults.Add(await GetCardLineResult(cardLine));
        result.PointsTotal = result.CardLineResults.Sum(c => c.PointsTotal);
        result.IsTurnValid = result.CardLineResults.All(r => r.IsValid);

        return result;
    }

    private async Task<CardLineResult> GetCardLineResult(List<Card> cardLine)
    {
        var cardsLinePoints = cardLine.OfType<LetterCard>().Select(c => c.Points).Sum();
        var cardIds = cardLine.Select(c => c.Id).ToList();
        var cardsLineWord = String.Join(
            "", 
            cardLine
                .OfType<LetterCard>() // for now assume all cards to be letter cards
                .Select(c => c.Letter));

        bool isAlreadyPlayed;
        if (HasPlayedWord(cardIds))
        {
            isAlreadyPlayed = true;
        }
        else
        {
            isAlreadyPlayed = false;
            PlayedWordHistory.Add(cardIds);
        }
        
        WordValidationResult validationResult = await _wordValidator.Validate(cardsLineWord);
        
        return new() {
            Word = validationResult.IsValid ? cardsLineWord : null,
            Definition = validationResult.IsValid ? validationResult.Definition : null,
            PointsTotal = isAlreadyPlayed ? 0 : cardsLinePoints,
            CardIds = cardIds,
            IsValid = validationResult.IsValid,
            IsAlreadyPlayed = isAlreadyPlayed
        };
    }
    
    private bool HasPlayedWord(List<int> cardIds)
    {
        return PlayedWordHistory.Any(history =>
            history.Count == cardIds.Count && !history.Except(cardIds).Any());
    }
    
    /// <summary>
    /// Given a card sequence, extract all 'card lines' of
    /// 2 or more consecutive indexes in that position sequence.
    ///
    /// sequences of 1 card are desregarded.
    /// </summary>
    private List<List<Card>> GetConsecutiveCardLines(
        IOrderedEnumerable<KeyValuePair<(int x, int y), Card>> cardSequence,
        bool isVertical)
    {
        List<List<Card>> cardLines = [];
            
        List<Card> runningCardLine = [];
        int? prevIndex = null;
        
        foreach (var (pos, card) in cardSequence)
        {
            int currentIndex = isVertical ? pos.y : pos.x;
            if (prevIndex == null || currentIndex == prevIndex + 1)
            {
                runningCardLine.Add(card);
                prevIndex = currentIndex;
            }
            else
            {
                // line of consecutive cards ended, so add if it is a word
                if (runningCardLine.Count > 1)
                    cardLines.Add(runningCardLine);
                    
                runningCardLine = new List<Card>();
                prevIndex = null;
            }
        }
        
        // add final word because the 'else' above won't be hit for it
        if (runningCardLine.Count > 1)
            cardLines.Add(runningCardLine);

        return cardLines;
    }
}

public class PlayCardsResult
{
    public List<CardLineResult> CardLineResults { get; set; } = new();
    public bool IsTurnValid { get; set; }
    public int PointsTotal { get; set; } = 0;
}

public class CardLineResult
{
    public List<int> CardIds { get; set; } = new();
    public bool IsAlreadyPlayed { get; set; }
    public bool IsValid { get; set; }
    public int PointsTotal { get; set; } = 0;
    public string? Word;
    public string? Definition;
}