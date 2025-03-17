using P1.Models;

namespace P1.Services;

public interface IDeckService
{
    List<Card> Cards { get;  }
    Card Pop();
}

public class DeckService : IDeckService
{
    public DeckService()
    {
        Cards = GetScrabbleDeck();
    }

    public DeckService(List<Card> deckCards)
    {
        Cards = deckCards;
    }
    
    public List<Card> Cards { get; private set; }
    public Card Pop()
    {
        var card = Cards[0];
        Cards.Remove(card);
        return card;
    }
    
    public static List<Card> GetScrabbleDeck()
    {
        var scrabbleDistribution = new List<(char Letter, int Points, int Frequency)>
        {
            ('a', 1, 9), ('b', 3, 2), ('c', 3, 2), ('d', 2, 4), ('e', 1, 12),
            ('f', 4, 2), ('g', 2, 3), ('h', 4, 2), ('i', 1, 9), ('j', 8, 1),
            ('k', 5, 1), ('l', 1, 4), ('m', 3, 2), ('n', 1, 6), ('o', 1, 8),
            ('p', 3, 2), ('q', 10, 1), ('r', 1, 6), ('s', 1, 4), ('t', 1, 6),
            ('u', 1, 4), ('v', 4, 2), ('w', 4, 2), ('x', 8, 1), ('y', 4, 2),
            ('z', 10, 1)
        };

        var deck = new List<Card>();
        foreach (var (letter, points, frequency) in scrabbleDistribution)
        {
            for (int i = 0; i < frequency; i++)
            {
                deck.Add(new LetterCard
                {
                    Id =  Guid.NewGuid().GetHashCode(),
                    Letter = letter,
                    Points = points
                });
            }
        }
        
        // randomize deck
        Random rng = new Random();  
        int n = deck.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            Card value = deck[k];  
            deck[k] = deck[n];  
            deck[n] = value;  
        }

        return deck;
    }

}