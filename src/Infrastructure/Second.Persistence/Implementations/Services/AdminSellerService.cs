using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Exceptions;
using Second.Domain.Enums;

namespace Second.Persistence.Implementations.Services
{
    public class AdminSellerService : IAdminSellerService
    {
        private readonly IUserRepository _userRepository;

        public AdminSellerService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> UpdateSellerListingLimitAsync(Guid sellerUserId, int listingLimit, CancellationToken cancellationToken = default)
        {
            var seller = await _userRepository.GetByIdAsync(sellerUserId, cancellationToken: cancellationToken);
            if (seller is null)
            {
                throw new NotFoundAppException("Seller user not found.", "seller_not_found");
            }

            if (seller.Role != UserRole.Seller && seller.Role != UserRole.Admin)
            {
                throw new BadRequestAppException("The specified user is not a seller.", "user_not_seller");
            }

            seller.ListingLimit = listingLimit;
            await _userRepository.UpdateAsync(seller, cancellationToken);

            return new UserDto
            {
                Id = seller.Id,
                Email = seller.Email,
                Role = seller.Role,
                EmailVerified = seller.EmailVerified,
                SellerRating = seller.SellerRating,
                ListingLimit = seller.ListingLimit,
                CreatedAt = seller.CreatedAt
            };
        }
    }
}
