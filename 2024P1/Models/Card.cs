namespace P1.Models;

public class Card
{
    public int Id;
}
public class LetterCard: Card
{
    public char Letter { get; set; }
}