using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Application.Exceptions;
using Second.Application.Models;
using Second.Domain.Entities;
using Second.Domain.Enums;

namespace Second.Persistence.Implementations.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IEntityValidationService _entityValidationService;
        private readonly IUserRepository _userRepository;

        public ProductService(
            IProductRepository productRepository,
            IProductImageRepository productImageRepository,
            IEntityValidationService entityValidationService,
            IUserRepository userRepository)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
            _entityValidationService = entityValidationService;
            _userRepository = userRepository;
        }

        public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        {
            var sellerExists = await _entityValidationService
                .SellerUserExistsAsync(request.SellerUserId, cancellationToken);

            if (!sellerExists)
            {
                throw new NotFoundAppException($"Seller user {request.SellerUserId} was not found.", "seller_not_found");
            }

            var seller = await _userRepository.GetByIdAsync(request.SellerUserId, cancellationToken: cancellationToken);
            if (seller is null)
            {
                throw new NotFoundAppException($"Seller user {request.SellerUserId} was not found.", "seller_not_found");
            }

            var hasListingCapacity = await _entityValidationService
                .SellerHasCapacityForActiveListingAsync(request.SellerUserId, seller.ListingLimit, excludedProductId: null, cancellationToken);
            if (!hasListingCapacity)
            {
                throw new ConflictAppException($"Seller reached active listing limit ({seller.ListingLimit}).", "listing_limit_reached");
            }

            var product = new Product
            {
                SellerUserId = request.SellerUserId,
                Title = request.Title,
                Description = request.Description,
                PriceText = request.PriceText,
                Price = request.Price,
                Condition = request.Condition,
                Status = ProductStatus.Active
            };

            foreach (var (url, index) in request.ImageUrls.Select((imageUrl, order) => (imageUrl, order)))
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = url,
                    Order = index,
                    Product = product
                });
            }

            await _productRepository.AddAsync(product, cancellationToken);

            return MapProduct(product);
        }

        public async Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            return product is null ? null : MapProduct(product);
        }

        public async Task<PagedResult<ProductDto>> GetBySellerUserIdAsync(
            Guid sellerUserId,
            PageRequest pageRequest,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _productRepository.GetBySellerUserIdAsync(
                sellerUserId,
                pageRequest.Skip,
                pageRequest.PageSize,
                cancellationToken);

            return new PagedResult<ProductDto>
            {
                Items = items.Select(MapProduct).ToList(),
                PageNumber = pageRequest.PageNumber,
                PageSize = pageRequest.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageRequest.PageSize)
            };
        }

        public async Task<PagedResult<ProductDto>> GetActiveAsync(
            PageRequest pageRequest,
            CancellationToken cancellationToken = default)
        {
            return await GetActiveAsync(new GetActiveProductsRequest(), pageRequest, cancellationToken);
        }

        public async Task<PagedResult<ProductDto>> GetActiveAsync(
            GetActiveProductsRequest request,
            PageRequest pageRequest,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _productRepository.GetActiveAsync(
                request,
                pageRequest.Skip,
                pageRequest.PageSize,
                cancellationToken);

            return new PagedResult<ProductDto>
            {
                Items = items.Select(MapProduct).ToList(),
                PageNumber = pageRequest.PageNumber,
                PageSize = pageRequest.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageRequest.PageSize)
            };
        }

        public async Task<ProductDto> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
        {
            var existingProduct = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

            if (existingProduct is null)
            {
                throw new NotFoundAppException("Product not found.", "product_not_found");
            }

            if (request.Status == ProductStatus.Active)
            {
                var seller = await _userRepository.GetByIdAsync(existingProduct.SellerUserId, cancellationToken: cancellationToken);
                if (seller is null)
                {
                    throw new NotFoundAppException($"Seller user {existingProduct.SellerUserId} was not found.", "seller_not_found");
                }

                var hasListingCapacity = await _entityValidationService
                    .SellerHasCapacityForActiveListingAsync(existingProduct.SellerUserId, seller.ListingLimit, existingProduct.Id, cancellationToken);
                if (!hasListingCapacity)
                {
                    throw new ConflictAppException($"Seller reached active listing limit ({seller.ListingLimit}).", "listing_limit_reached");
                }
            }

            existingProduct.Title = request.Title;
            existingProduct.Description = request.Description;
            existingProduct.PriceText = request.PriceText;
            existingProduct.Price = request.Price;
            existingProduct.Condition = request.Condition;
            existingProduct.Status = request.Status;

            await _productRepository.UpdateAsync(existingProduct, cancellationToken);

            return MapProduct(existingProduct);
        }

        public async Task<ProductImageDto> AddImageAsync(AddProductImageRequest request, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

            if (product is null)
            {
                throw new NotFoundAppException("Product not found.", "product_not_found");
            }

            var image = new ProductImage
            {
                ProductId = request.ProductId,
                ImageUrl = request.ImageUrl,
                Order = request.Order
            };

            await _productImageRepository.AddAsync(image, cancellationToken);

            return MapProductImage(image);
        }

        public async Task RemoveImageAsync(Guid imageId, CancellationToken cancellationToken = default)
        {
            var image = await _productImageRepository.GetByIdAsync(imageId, cancellationToken);

            if (image is null)
            {
                return;
            }

            await _productImageRepository.RemoveAsync(image, cancellationToken);
        }

        public async Task DeleteAsync(Guid productId, Guid actorUserId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product is null)
            {
                return;
            }

            if (!isAdmin && product.SellerUserId != actorUserId)
            {
                throw new ForbiddenAppException("You are not allowed to archive this product.", "product_delete_forbidden");
            }

            product.Status = ProductStatus.Archived;
            await _productRepository.UpdateAsync(product, cancellationToken);
        }

        private static ProductDto MapProduct(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                SellerUserId = product.SellerUserId,
                Title = product.Title,
                Description = product.Description,
                PriceText = product.PriceText,
                Price = product.Price,
                Condition = product.Condition,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Images = product.Images
                    .OrderBy(image => image.Order)
                    .Select(MapProductImage)
                    .ToList()
            };
        }

        private static ProductImageDto MapProductImage(ProductImage image)
        {
            return new ProductImageDto
            {
                Id = image.Id,
                ProductId = image.ProductId,
                ImageUrl = image.ImageUrl,
                Order = image.Order
            };
        }
    }
}
