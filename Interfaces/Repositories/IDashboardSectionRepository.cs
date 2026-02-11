using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface IDashboardSectionRepository : IRepository<DashboardSection>
{
    Task<IEnumerable<DashboardSection>> GetVisibleOrderedAsync();
}
