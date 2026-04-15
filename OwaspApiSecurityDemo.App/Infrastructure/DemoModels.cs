using System;

namespace OwaspApiSecurityDemo.App.Infrastructure
{
    public sealed class DemoUser
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }
    }

    public sealed class DemoOrder
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Item { get; set; }

        public decimal Amount { get; set; }
    }

    public sealed class LoginRequest
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    public sealed class TokenValidationRequest
    {
        public string Token { get; set; }
    }

    public sealed class TokenPrincipal
    {
        public int UserId { get; set; }

        public string Subject { get; set; }

        public string Role { get; set; }

        public DateTime? ExpiresUtc { get; set; }
    }
}
