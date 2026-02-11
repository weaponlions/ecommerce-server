using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class SocialIconRepository : Repository<SocialIcon>, ISocialIconRepository
{
    public SocialIconRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<SocialIcon>> GetVisibleOrderedAsync()
        => await _dbSet
            .Where(s => s.IsVisible)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();
}
