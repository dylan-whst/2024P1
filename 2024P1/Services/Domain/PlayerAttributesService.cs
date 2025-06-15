namespace P1.Services;

public interface IPlayerAttributesService
{
    int Money { get; set; }
    int Hands { get; set; }
    int Discards { get; set; }
}

public class PlayerAttributesService: IPlayerAttributesService
{
    public int Money { get; set; } = 0;
    public int Hands { get; set; } = 4;
    public int Discards { get; set; } = 2;
}