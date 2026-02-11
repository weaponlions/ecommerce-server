using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class FooterLinkRepository : Repository<FooterLink>, IFooterLinkRepository
{
    public FooterLinkRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<FooterLink>> GetVisibleOrderedAsync()
        => await _dbSet
            .Where(l => l.IsVisible)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();
}
