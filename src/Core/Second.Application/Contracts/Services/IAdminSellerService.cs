using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Dtos;

namespace Second.Application.Contracts.Services
{
    public interface IAdminSellerService
    {
        Task<UserDto> UpdateSellerListingLimitAsync(Guid sellerUserId, int listingLimit, CancellationToken cancellationToken = default);
    }
}
