using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface ICarouselSlideRepository : IRepository<CarouselSlide>
{
    Task<IEnumerable<CarouselSlide>> GetActiveSlidesAsync(DateTime now);
}
