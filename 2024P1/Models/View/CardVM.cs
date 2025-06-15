using System.Text.RegularExpressions;

namespace P1.Models;

public class CardVM
{

    public CardVM() { }
    
    public CardVM(Card card, CardPlace place)
    {
        var rng = new Random();
        if (card is LetterCard letterCard)
        {

            Superscript = letterCard.Points.ToString();
            Text = letterCard.Letter.ToString();
            Place = place;
            Id = letterCard.Id;
            HandOrder = rng.Next();
        }
        else
        {
            throw new NotSupportedException($"could not create view for card '{card.Id}': it is of an unsupported type");
        }
    }
    
    public int Id { get; set; }
    public string Name { get; set; }
    public string Text { get; set; }
    public string? Superscript { get; set; }
    public CardPlace Place { get; set; }
    private CardHighlight _highlight = CardHighlight.None;

    public CardHighlight Highlight
    {
        get
        {
            if (_highlight == CardHighlight.Success || _highlight == CardHighlight.Failure)
            {
                return _highlight;
            }
            
            return IsCemented ? CardHighlight.Cemented : _highlight;
        }
        set
        {
            _highlight = value;
        }
    }
    public string? HoverText { get; set; }
    public bool IsCemented { get; set; } = false;
    
    public int HandOrder { get; set; }
}

public class CardPlace
{
    
    public CardPlace() {}
    
    public CardPlace(bool isHand, (int x, int y)? boardPos)
    {
        IsHand = isHand;
        BoardPos = boardPos;
    }
    
    public CardPlace(string dropzoneRep)
    {
        if (dropzoneRep == "hand")
            IsHand = true;
        else
        {
            IsHand = false;
            var match = Regex.Match(dropzoneRep, @"^board-\((-?\d+),(-?\d+)\)$");
            var pos = (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
            BoardPos = pos;
        }
    }
    
    public bool IsHand { get; set; }
    public (int x, int y)? BoardPos { get; set; }

    public override string ToString()
    {
        if (IsHand)
            return "hand";
        else
            return $"board-({BoardPos.Value.x},{BoardPos.Value.y})";
    }

    public static CardPlace InHand => new CardPlace() { IsHand = true };
}

public enum CardHighlight
{
    Success,
    Failure,
    None,
    Cemented
}
