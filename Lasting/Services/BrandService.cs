using Lasting.Models;
using Lasting.Repositories;
using Lasting.Services;

namespace Lasting.Services
{
    public class BrandService : IBrandService
    {
        private readonly IRepository<Brand> _brandRepository;

        public BrandService(IRepository<Brand> brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
        {
            return await _brandRepository.GetAllAsync();
        }

        public async Task<Brand?> GetBrandByIdAsync(int id)
        {
            return await _brandRepository.GetByIdAsync(id);
        }

        public async Task AddBrandAsync(Brand brand)
        {
            await _brandRepository.AddAsync(brand);
            await _brandRepository.SaveChangesAsync();
        }

        public async Task UpdateBrandAsync(Brand brand)
        {
            _brandRepository.Update(brand);
            await _brandRepository.SaveChangesAsync();
        }

        public async Task DeleteBrandAsync(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand != null)
            {
                _brandRepository.Delete(brand);
                await _brandRepository.SaveChangesAsync();
            }
        }
    }
}