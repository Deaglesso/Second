using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class UpdateSellerListingLimitRequestValidator : AbstractValidator<UpdateSellerListingLimitRequest>
    {
        public UpdateSellerListingLimitRequestValidator()
        {
            RuleFor(request => request.ListingLimit)
                .GreaterThanOrEqualTo(0);
        }
    }
}
