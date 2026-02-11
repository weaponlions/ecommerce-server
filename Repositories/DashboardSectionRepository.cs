using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class DashboardSectionRepository : Repository<DashboardSection>, IDashboardSectionRepository
{
    public DashboardSectionRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<DashboardSection>> GetVisibleOrderedAsync()
        => await _dbSet
            .Where(s => s.IsVisible)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();
}
