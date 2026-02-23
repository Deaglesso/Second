using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Second.API.Models;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Application.Exceptions;
using Second.Application.Models;

namespace Second.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [Authorize(Policy = "SellerOnly")]
        public async Task<ActionResult<ProductDto>> CreateAsync(
            [FromBody] CreateProductRequest request,
            CancellationToken cancellationToken)
        {
            var product = await _productService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetByIdAsync), new { productId = product.Id }, product);
        }

        [HttpGet("{productId:guid}")]
        [AllowAnonymous]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<ActionResult<ProductDto>> GetByIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            var product = await _productService.GetByIdAsync(productId, cancellationToken);
            if (product is null)
            {
                throw new NotFoundAppException($"No product found with id {productId}.", "product_not_found");
            }

            return Ok(product);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetActiveAsync(
            [FromQuery] PaginationParameters pagination,
            [FromQuery] ActiveProductsQueryParameters query,
            CancellationToken cancellationToken)
        {
            ValidatePagination(pagination);
            ValidateActiveProductsQuery(query);

            var pageRequest = new PageRequest { PageNumber = pagination.PageNumber, PageSize = pagination.PageSize };
            var request = new GetActiveProductsRequest
            {
                Query = query.Q,
                Condition = query.Condition,
                MinPrice = query.MinPrice,
                MaxPrice = query.MaxPrice,
                SortBy = query.SortBy
            };

            var products = await _productService.GetActiveAsync(request, pageRequest, cancellationToken);
            return Ok(products);
        }

        [HttpGet("by-seller/{sellerUserId:guid}")]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetBySellerAsync(
            Guid sellerUserId,
            [FromQuery] PaginationParameters pagination,
            CancellationToken cancellationToken)
        {
            ValidatePagination(pagination);

            var pageRequest = new PageRequest { PageNumber = pagination.PageNumber, PageSize = pagination.PageSize };
            var products = await _productService.GetBySellerUserIdAsync(sellerUserId, pageRequest, cancellationToken);
            return Ok(products);
        }

        [HttpPut("{productId:guid}")]
        [Authorize(Policy = "SellerOnly")]
        public async Task<ActionResult<ProductDto>> UpdateAsync(
            Guid productId,
            [FromBody] UpdateProductRequest request,
            CancellationToken cancellationToken)
        {
            var updatedRequest = request with { ProductId = productId };
            var product = await _productService.UpdateAsync(updatedRequest, cancellationToken);
            return Ok(product);
        }

        [HttpPost("{productId:guid}/images")]
        [Authorize(Policy = "SellerOnly")]
        public async Task<ActionResult<ProductImageDto>> AddImageAsync(
            Guid productId,
            [FromBody] AddProductImageRequest request,
            CancellationToken cancellationToken)
        {
            var updatedRequest = request with { ProductId = productId };
            var image = await _productService.AddImageAsync(updatedRequest, cancellationToken);
            return Ok(image);
        }

        [HttpDelete("images/{imageId:guid}")]
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> RemoveImageAsync(Guid imageId, CancellationToken cancellationToken)
        {
            await _productService.RemoveImageAsync(imageId, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{productId:guid}")]
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> DeleteAsync(Guid productId, CancellationToken cancellationToken)
        {
            var actorUserId = GetAuthenticatedUserId();
            var isAdmin = User.IsInRole("Admin");
            await _productService.DeleteAsync(productId, actorUserId, isAdmin, cancellationToken);
            return NoContent();
        }

        private static void ValidatePagination(PaginationParameters pagination)
        {
            if (pagination.IsValid())
            {
                return;
            }

            throw new BadRequestAppException(
                $"PageNumber must be >= 1 and PageSize must be between 1 and {PaginationParameters.MaxPageSize}.",
                "invalid_pagination_parameters");
        }

        private static void ValidateActiveProductsQuery(ActiveProductsQueryParameters query)
        {
            if (query.MinPrice.HasValue && query.MinPrice.Value < 0)
            {
                throw new BadRequestAppException("minPrice must be greater than or equal to 0.", "invalid_min_price");
            }

            if (query.MaxPrice.HasValue && query.MaxPrice.Value < 0)
            {
                throw new BadRequestAppException("maxPrice must be greater than or equal to 0.", "invalid_max_price");
            }

            if (query.MinPrice.HasValue && query.MaxPrice.HasValue && query.MinPrice.Value > query.MaxPrice.Value)
            {
                throw new BadRequestAppException("minPrice cannot be greater than maxPrice.", "invalid_price_range");
            }

            var sortBy = query.SortBy.ToLowerInvariant();
            if (sortBy != "newest" && sortBy != "price_asc" && sortBy != "price_desc")
            {
                throw new BadRequestAppException("sortBy must be one of: newest, price_asc, price_desc.", "invalid_sort_by");
            }
        }

        private Guid GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAppException("Invalid user token.", "invalid_user_token");
            }

            return userId;
        }
    }
}
