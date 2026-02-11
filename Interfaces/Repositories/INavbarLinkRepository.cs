using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface INavbarLinkRepository : IRepository<NavbarLink>
{
    Task<IEnumerable<NavbarLink>> GetVisibleOrderedAsync();
}
