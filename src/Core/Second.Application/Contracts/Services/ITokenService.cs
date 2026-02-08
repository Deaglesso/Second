using System;
using Second.Domain.Entities;

namespace Second.Application.Contracts.Services
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
    }
}
