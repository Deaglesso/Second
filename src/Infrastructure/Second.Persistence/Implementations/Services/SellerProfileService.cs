using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Application.Models;
using Second.Domain.Entities;
using Second.Domain.Enums;

namespace Second.Persistence.Implementations.Services
{
    public class SellerProfileService : ISellerProfileService
    {
        private readonly ISellerProfileRepository _sellerProfileRepository;

        public SellerProfileService(ISellerProfileRepository sellerProfileRepository)
        {
            _sellerProfileRepository = sellerProfileRepository;
        }

        public async Task<SellerProfileDto> CreateAsync(CreateSellerProfileRequest request, CancellationToken cancellationToken = default)
        {
            var userId = request.UserId == Guid.Empty ? Guid.NewGuid() : request.UserId;
            var profile = new SellerProfile
            {
                UserId = userId,
                DisplayName = request.DisplayName,
                Bio = request.Bio,
                Status = SellerStatus.Pending
            };

            await _sellerProfileRepository.AddAsync(profile, cancellationToken);

            return MapProfile(profile);
        }

        public async Task<SellerProfileDto?> GetByIdAsync(Guid sellerProfileId, CancellationToken cancellationToken = default)
        {
            var profile = await _sellerProfileRepository.GetByIdAsync(sellerProfileId, cancellationToken);
            return profile is null ? null : MapProfile(profile);
        }

        public async Task<SellerProfileDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var profile = await _sellerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
            return profile is null ? null : MapProfile(profile);
        }

        public async Task<PagedResult<SellerProfileDto>> GetAllAsync(
            PageRequest pageRequest,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _sellerProfileRepository.GetAllAsync(
                pageRequest.Skip,
                pageRequest.PageSize,
                cancellationToken);

            return new PagedResult<SellerProfileDto>
            {
                Items = items.Select(MapProfile).ToList(),
                PageNumber = pageRequest.PageNumber,
                PageSize = pageRequest.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageRequest.PageSize)
            };
        }

        public async Task<SellerProfileDto> UpdateAsync(UpdateSellerProfileRequest request, CancellationToken cancellationToken = default)
        {
            var profile = await _sellerProfileRepository.GetByIdAsync(request.SellerProfileId, cancellationToken);

            if (profile is null)
            {
                throw new InvalidOperationException("Seller profile not found.");
            }

            profile.DisplayName = request.DisplayName;
            profile.Bio = request.Bio;
            profile.Status = request.Status;

            await _sellerProfileRepository.UpdateAsync(profile, cancellationToken);

            return MapProfile(profile);
        }

        private static SellerProfileDto MapProfile(SellerProfile profile)
        {
            return new SellerProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                DisplayName = profile.DisplayName,
                Bio = profile.Bio,
                Status = profile.Status,
                CreatedAt = profile.CreatedAt
            };
        }
    }
}
