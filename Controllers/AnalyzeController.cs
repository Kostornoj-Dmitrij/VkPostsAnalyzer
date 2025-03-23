using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> AnalyzePosts([FromQuery] string vkPageUrl)
    {
        _logger.LogInformation("Starting analysis");

        var text = await _vkService.GetPostsTextAsync(vkPageUrl);

        if (string.IsNullOrEmpty(text))
        {
            _logger.LogWarning("No text posts found on the page.");
            return NotFound("No text posts found.");
        }

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