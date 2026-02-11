using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class NavbarLinkRepository : Repository<NavbarLink>, INavbarLinkRepository
{
    public NavbarLinkRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<NavbarLink>> GetVisibleOrderedAsync()
        => await _dbSet
            .Where(l => l.IsVisible)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();
}
