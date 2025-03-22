using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using VkPostsAnalyzer.Data;
using VkPostsAnalyzer.Models;
using VkPostsAnalyzer.Services;

namespace VkPostsAnalyzer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyzeController : ControllerBase
{
    private readonly VkService _vkService;
    private readonly AppDbContext _context;
    private readonly ILogger<AnalyzeController> _logger;

    public AnalyzeController(
        VkService vkService, 
        AppDbContext context,
        ILogger<AnalyzeController> logger)
    {
        _vkService = vkService;
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzePosts([FromQuery] string accessToken, [FromQuery] int userId)
    {
        _logger.LogInformation("Starting analysis");

        var postsJson = await _vkService.GetPostsTextAsync(accessToken, userId);
        var response = JObject.Parse(postsJson)["response"];
        var posts = response["items"].ToObject<List<JObject>>();

        if (posts == null || !posts.Any())
        {
            _logger.LogWarning("No posts found for user {UserId}", userId);
            return NotFound("No posts found.");
        }

        var texts = posts.Select(p => p["text"]?.ToString()).ToList();
        var text = string.Join(" ", texts.Where(t => !string.IsNullOrEmpty(t)));

        var letterCounts = ProcessText(text);
        await SaveResultsAsync(letterCounts);

        _logger.LogInformation("Analysis completed");
        return Ok(letterCounts);
    }

    private Dictionary<char, int> ProcessText(string text)
    {
        return text
            .Where(char.IsLetter)
            .Select(char.ToLower)
            .GroupBy(c => c)
            .ToDictionary(g => g.Key, g => g.Count())
            .OrderBy(p => p.Key)
            .ToDictionary(p => p.Key, p => p.Value);
    }

    private async Task SaveResultsAsync(Dictionary<char, int> letterCounts)
    {
        var entities = letterCounts.Select(p => new LetterCount
        {
            Letter = p.Key,
            Count = p.Value
        });

        await _context.LetterCounts.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }
}