using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;

namespace Second.Application.Contracts.Services
{
    public interface IProductService
    {
        Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

        Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductDto>> GetBySellerProfileIdAsync(Guid sellerProfileId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductDto>> GetActiveAsync(CancellationToken cancellationToken = default);

        Task<ProductDto> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default);

        Task<ProductImageDto> AddImageAsync(AddProductImageRequest request, CancellationToken cancellationToken = default);

        Task RemoveImageAsync(Guid imageId, CancellationToken cancellationToken = default);
    }
}
