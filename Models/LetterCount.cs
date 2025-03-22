namespace VkPostsAnalyzer.Models;

public class LetterCount
{
    public int Id { get; set; }
    public char Letter { get; set; }
    public int Count { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}