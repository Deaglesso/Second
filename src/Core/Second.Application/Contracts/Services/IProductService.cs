using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Application.Models;

namespace Second.Application.Contracts.Services
{
    public interface IProductService
    {
        Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

        Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);

        Task<PagedResult<ProductDto>> GetBySellerUserIdAsync(Guid sellerUserId, PageRequest pageRequest, CancellationToken cancellationToken = default);

        Task<PagedResult<ProductDto>> GetActiveAsync(PageRequest pageRequest, CancellationToken cancellationToken = default);

        Task<ProductDto> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default);

        Task<ProductImageDto> AddImageAsync(AddProductImageRequest request, CancellationToken cancellationToken = default);

        Task RemoveImageAsync(Guid imageId, CancellationToken cancellationToken = default);
    }
}
