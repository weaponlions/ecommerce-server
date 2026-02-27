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
            .Include(s => s.MediaAsset)
            .Where(s => s.IsVisible)
            .Where(s => (s.StartDate == null || s.StartDate <= now) &&
                        (s.EndDate == null || s.EndDate >= now))
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

    public override async Task<IEnumerable<CarouselSlide>> GetAllAsync()
        => await _dbSet
            .Include(s => s.MediaAsset)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

    public override async Task<CarouselSlide?> GetByIdAsync(int id)
        => await _dbSet
            .Include(s => s.MediaAsset)
            .FirstOrDefaultAsync(s => s.Id == id);
}
