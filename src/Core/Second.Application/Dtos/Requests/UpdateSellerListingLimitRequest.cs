namespace Second.Application.Dtos.Requests
{
    public sealed record UpdateSellerListingLimitRequest
    {
        public int ListingLimit { get; init; }
    }
}
