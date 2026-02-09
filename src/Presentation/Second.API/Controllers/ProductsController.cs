using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Second.API.Models;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Application.Models;

namespace Second.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = "SellerOnly")]
        public async Task<ActionResult<ProductDto>> CreateAsync(
            [FromBody] CreateProductRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var product = await _productService.CreateAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetByIdAsync), new { productId = product.Id }, product);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Product create failed for SellerUserId {SellerUserId}.", request.SellerUserId);
                return NotFound(CreateProblemDetails(
                    "Seller user not found.",
                    $"No seller user found with id {request.SellerUserId}."));
            }
        }

        [HttpGet("{productId:guid}")]
        [AllowAnonymous]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<ActionResult<ProductDto>> GetByIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            var product = await _productService.GetByIdAsync(productId, cancellationToken);
            if (product is null)
            {
                return NotFound(CreateProblemDetails("Product not found.", $"No product found with id {productId}."));
            }

            return Ok(product);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetActiveAsync(
            [FromQuery] PaginationParameters pagination,
            CancellationToken cancellationToken)
        {
            var validationResult = ValidatePagination(pagination);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var pageRequest = new PageRequest { PageNumber = pagination.PageNumber, PageSize = pagination.PageSize };
            var products = await _productService.GetActiveAsync(pageRequest, cancellationToken);
            return Ok(products);
        }

        [HttpGet("by-seller/{sellerUserId:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetBySellerAsync(
            Guid sellerUserId,
            [FromQuery] PaginationParameters pagination,
            CancellationToken cancellationToken)
        {
            var validationResult = ValidatePagination(pagination);
            if (validationResult is not null)
            {
                return validationResult;
            }

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
            try
            {
                var updatedRequest = request with { ProductId = productId };
                var product = await _productService.UpdateAsync(updatedRequest, cancellationToken);
                return Ok(product);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Product update failed for {ProductId}.", productId);
                return NotFound(CreateProblemDetails("Product not found.", $"No product found with id {productId}."));
            }
        }

        [HttpPost("{productId:guid}/images")]
        [Authorize(Policy = "SellerOnly")]
        public async Task<ActionResult<ProductImageDto>> AddImageAsync(
            Guid productId,
            [FromBody] AddProductImageRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var updatedRequest = request with { ProductId = productId };
                var image = await _productService.AddImageAsync(updatedRequest, cancellationToken);
                return Ok(image);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Add image failed for {ProductId}.", productId);
                return NotFound(CreateProblemDetails("Product not found.", $"No product found with id {productId}."));
            }
        }

        [HttpDelete("images/{imageId:guid}")]
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> RemoveImageAsync(Guid imageId, CancellationToken cancellationToken)
        {
            await _productService.RemoveImageAsync(imageId, cancellationToken);
            return NoContent();
        }

        private ActionResult? ValidatePagination(PaginationParameters pagination)
        {
            if (pagination.IsValid())
            {
                return null;
            }

            return BadRequest(CreateProblemDetails(
                "Invalid pagination parameters.",
                $"PageNumber must be >= 1 and PageSize must be between 1 and {PaginationParameters.MaxPageSize}."));
        }

        private static ProblemDetails CreateProblemDetails(string title, string detail)
        {
            return new ProblemDetails
            {
                Title = title,
                Detail = detail
            };
        }
    }
}
