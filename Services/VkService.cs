using PuppeteerSharp;

namespace VkPostsAnalyzer.Services;

public class VkService
{
    private readonly ILogger<VkService> _logger;

    public VkService(ILogger<VkService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetPostsTextAsync(string vkPageUrl)
    {
        _logger.LogInformation("Fetching posts from VK page: {Url}", vkPageUrl);

        await new BrowserFetcher().DownloadAsync();

        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });

        var page = await browser.NewPageAsync();

        await page.GoToAsync(vkPageUrl, WaitUntilNavigation.Networkidle2);

        var postsLimit = 5;
        await page.EvaluateFunctionAsync(@"
                (postsLimit) => {
                    const posts = document.querySelectorAll('.vkitShowMoreText__text');
                    if (posts.length > postsLimit) {
                        for (let i = postsLimit; i < posts.length; i++) {
                            posts[i].remove();
                        }
                    }
                }
            ", postsLimit);

        var html = await page.GetContentAsync();

        await browser.CloseAsync();
        
        var htmlDoc = new HtmlAgilityPack.HtmlDocument();
        htmlDoc.LoadHtml(html);

        var postTexts = new List<string>();
        var postNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'vkitShowMoreText__text')]");

        if (postNodes != null)
        {
            postsLimit = 5;
            for (int i = 0; i < Math.Min(postNodes.Count, postsLimit); i++)
            {
                var text = postNodes[i].InnerText.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    postTexts.Add(text);
                }
            }
        }

        if (!postTexts.Any())
        {
            _logger.LogWarning("No text posts found.");
            return string.Empty;
        }

        return string.Join(" ", postTexts);
    }
}