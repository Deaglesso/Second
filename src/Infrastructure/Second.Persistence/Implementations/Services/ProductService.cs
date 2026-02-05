using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Domain.Entities;

namespace Second.Persistence.Implementations.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;

        public ProductService(IProductRepository productRepository, IProductImageRepository productImageRepository)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
        }

        public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        {
            var product = new Product
            {
                SellerProfileId = request.SellerProfileId,
                Title = request.Title,
                Description = request.Description,
                PriceText = request.PriceText,
                Condition = request.Condition,
                IsActive = true,
                Images = request.ImageUrls
                    .Select((url, index) => new ProductImage
                    {
                        ImageUrl = url,
                        Order = index
                    })
                    .ToList()
            };

            await _productRepository.AddAsync(product, cancellationToken);

            return MapProduct(product);
        }

        public async Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            return product is null ? null : MapProduct(product);
        }

        public async Task<IReadOnlyList<ProductDto>> GetBySellerProfileIdAsync(Guid sellerProfileId, CancellationToken cancellationToken = default)
        {
            var products = await _productRepository.GetBySellerProfileIdAsync(sellerProfileId, cancellationToken);
            return products.Select(MapProduct).ToList();
        }

        public async Task<IReadOnlyList<ProductDto>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            var products = await _productRepository.GetActiveAsync(cancellationToken);
            return products.Select(MapProduct).ToList();
        }

        public async Task<ProductDto> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
        {
            var existingProduct = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

            if (existingProduct is null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            existingProduct.Title = request.Title;
            existingProduct.Description = request.Description;
            existingProduct.PriceText = request.PriceText;
            existingProduct.Condition = request.Condition;
            existingProduct.IsActive = request.IsActive;

            await _productRepository.UpdateAsync(existingProduct, cancellationToken);

            return MapProduct(existingProduct);
        }

        public async Task<ProductImageDto> AddImageAsync(AddProductImageRequest request, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

            if (product is null)
            {
                throw new InvalidOperationException("Product not found.");
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

        private static ProductDto MapProduct(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                SellerProfileId = product.SellerProfileId,
                Title = product.Title,
                Description = product.Description,
                PriceText = product.PriceText,
                Condition = product.Condition,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
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
