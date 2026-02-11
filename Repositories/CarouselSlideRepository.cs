using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class CarouselSlideRepository : Repository<CarouselSlide>, ICarouselSlideRepository
{
    public CarouselSlideRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<CarouselSlide>> GetActiveSlidesAsync(DateTime now)
        => await _dbSet
            .Where(s => s.IsVisible)
            .Where(s => (s.StartDate == null || s.StartDate <= now) &&
                        (s.EndDate == null || s.EndDate >= now))
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();
}
