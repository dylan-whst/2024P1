using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public bool IsValid { get; }
    public string Definition { get; }
}

public class DictionaryApiWordValidator : IWordValidator
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public async Task<WordValidationResult> Validate(string word)
    {
        var url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var entries = JsonSerializer.Deserialize<List<DictionaryResponse>>(jsonResponse);

            if (entries != null && entries.Count > 0)
            {
                // Simplified: Extract the first definition from the first meaning
                var firstEntry = entries[0];
                var firstDefinition = firstEntry.Meanings.FirstOrDefault()?.Definitions.FirstOrDefault()?.Text ?? "Definition not found.";

                return new WordValidationResult(true, firstDefinition);
            }
        }

        return new WordValidationResult(false, "Word not found.");
    }
}

public class DictionaryResponse
{
    [JsonPropertyName("word")]
    public string Word { get; set; }
    
    [JsonPropertyName("phonetic")]
    public string Phonetic { get; set; }
    
    [JsonPropertyName("phonetics")]
    public List<Phonetic> Phonetics { get; set; }
    
    [JsonPropertyName("meanings")]
    public List<Meaning> Meanings { get; set; }
    
    [JsonPropertyName("license")]
    public License License { get; set; }
    
    [JsonPropertyName("sourceUrls")]
    public List<string> SourceUrls { get; set; }
}

public class Phonetic
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
    
    [JsonPropertyName("audio")]
    public string Audio { get; set; }
    
    [JsonPropertyName("sourceUrl")]
    public string SourceUrl { get; set; }
    
    [JsonPropertyName("license")]
    public License License { get; set; }
}

public class Meaning
{
    [JsonPropertyName("partOfSpeech")]
    public string PartOfSpeech { get; set; }
    
    [JsonPropertyName("definitions")]
    public List<Definition> Definitions { get; set; }
    
    [JsonPropertyName("synonyms")]
    public List<string> Synonyms { get; set; }
    
    [JsonPropertyName("antonyms")]
    public List<string> Antonyms { get; set; }
}

public class Definition
{
    [JsonPropertyName("definition")]
    public string Text { get; set; }
    
    [JsonPropertyName("synonyms")]
    public List<string> Synonyms { get; set; }
    
    [JsonPropertyName("antonyms")]
    public List<string> Antonyms { get; set; }
    
    [JsonPropertyName("example")]
    public string Example { get; set; } // Optional, based on JSON
}

public class License
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

