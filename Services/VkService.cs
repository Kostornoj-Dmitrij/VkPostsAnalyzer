using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkPostsAnalyzer.Models;

namespace VkPostsAnalyzer.Services;

public class VkService
{
    private readonly ILogger<VkService> _logger;

    public VkService(ILogger<VkService> logger)
    {
        _logger = logger;
    }

    public Task<string> GetPostsTextAsync(string accessToken, int userId, int count = 5)
    {
        _logger.LogInformation("Fetching mock posts from VK");

        var mockPosts = new
        {
            response = new
            {
                items = new[]
                {
                    new { text = "HelLo world! This is a test post." },
                    new { text = "C# is aWeSoMe" },
                    new { text = "VK API is cool, but we're using mocks for now." },
                    new { text = "4 post for testing purposes." },
                    new { text = "This is the fifth post." },
                    new { text = "6 post." },
                    new { text = "7 post." },
                    new { text = "8 post." }
                }
            }
        };

        var json = JsonConvert.SerializeObject(mockPosts);
        return Task.FromResult(json);
    }
}