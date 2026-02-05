using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;

namespace Second.Application.Contracts.Services
{
    public interface ISellerProfileService
    {
        Task<SellerProfileDto> CreateAsync(CreateSellerProfileRequest request, CancellationToken cancellationToken = default);

        Task<SellerProfileDto?> GetByIdAsync(Guid sellerProfileId, CancellationToken cancellationToken = default);

        Task<SellerProfileDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<SellerProfileDto> UpdateAsync(UpdateSellerProfileRequest request, CancellationToken cancellationToken = default);
    }
}
