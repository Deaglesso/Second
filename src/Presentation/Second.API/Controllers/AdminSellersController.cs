using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;

namespace Second.API.Controllers
{
    [ApiController]
    [Route("api/v1/admin/sellers")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public class AdminSellersController : ControllerBase
    {
        private readonly IAdminSellerService _adminSellerService;

        public AdminSellersController(IAdminSellerService adminSellerService)
        {
            _adminSellerService = adminSellerService;
        }

        [HttpPatch("{sellerUserId:guid}/listing-limit")]
        public async Task<ActionResult<UserDto>> UpdateSellerListingLimitAsync(
            Guid sellerUserId,
            [FromBody] UpdateSellerListingLimitRequest request,
            CancellationToken cancellationToken)
        {
            var updatedSeller = await _adminSellerService.UpdateSellerListingLimitAsync(sellerUserId, request.ListingLimit, cancellationToken);
            return Ok(updatedSeller);
        }
    }
}
