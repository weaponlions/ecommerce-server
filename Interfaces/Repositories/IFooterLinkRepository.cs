using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface IFooterLinkRepository : IRepository<FooterLink>
{
    Task<IEnumerable<FooterLink>> GetVisibleOrderedAsync();
}
