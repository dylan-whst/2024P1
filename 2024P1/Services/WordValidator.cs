namespace P1.Services;

public interface IWordValidator
{
    Task<WordValidationResult> Validate(string word);
}

public class WordValidationResult
{
    public WordValidationResult(bool isValid, string definition)
    {
        IsValid = isValid;
        Definition = definition;
    }

    bool IsValid { get; }
    string Definition { get; }
}