using Microsoft.EntityFrameworkCore;
using VkPostsAnalyzer.Models;

namespace VkPostsAnalyzer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<LetterCount> LetterCounts { get; set; }
}