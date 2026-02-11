using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface ISocialIconRepository : IRepository<SocialIcon>
{
    Task<IEnumerable<SocialIcon>> GetVisibleOrderedAsync();
}
