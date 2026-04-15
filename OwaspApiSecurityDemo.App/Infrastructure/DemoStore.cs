using System;
using System.Collections.Generic;
using System.Linq;

namespace OwaspApiSecurityDemo.App.Infrastructure
{
    public static class DemoStore
    {
        private static readonly object AuthGate = new object();
        private static readonly List<DemoUser> Users = new List<DemoUser>
        {
            new DemoUser { Id = 1, UserName = "alice", Password = "alice123", Role = "user" },
            new DemoUser { Id = 2, UserName = "bob", Password = "bob123", Role = "user" },
            new DemoUser { Id = 3, UserName = "admin", Password = "admin123", Role = "admin" }
        };

        private static readonly List<DemoOrder> Orders = new List<DemoOrder>
        {
            new DemoOrder { Id = 101, UserId = 1, Item = "Savings Account Kit", Amount = 1000m },
            new DemoOrder { Id = 102, UserId = 1, Item = "Debit Card Reissue", Amount = 250m },
            new DemoOrder { Id = 201, UserId = 2, Item = "Loan Statement Export", Amount = 500m },
            new DemoOrder { Id = 301, UserId = 3, Item = "Privileged Audit Export", Amount = 0m }
        };

        private static readonly Dictionary<string, FailedLoginState> FailedLogins =
            new Dictionary<string, FailedLoginState>(StringComparer.OrdinalIgnoreCase);

        public static DemoUser FindUserById(int id)
        {
            return Users.FirstOrDefault(user => user.Id == id);
        }

        public static DemoUser FindUserByUserName(string username)
        {
            return Users.FirstOrDefault(user => user.UserName.Equals(username ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        }

        public static bool ValidateCredentials(string username, string password, out DemoUser user)
        {
            user = Users.FirstOrDefault(candidate =>
                candidate.UserName.Equals(username ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
                candidate.Password == (password ?? string.Empty));

            return user != null;
        }

        public static bool TryAuthenticateSecure(string username, string password, DateTime utcNow, out DemoUser user, out string message)
        {
            user = null;
            message = null;

            lock (AuthGate)
            {
                var state = GetState(username);

                if (state.LockedUntilUtc.HasValue && state.LockedUntilUtc.Value > utcNow)
                {
                    message = "Account is temporarily locked because of repeated failed login attempts.";
                    return false;
                }

                if (ValidateCredentials(username, password, out user))
                {
                    state.Failures.Clear();
                    state.LockedUntilUtc = null;
                    return true;
                }

                state.Failures.Add(utcNow);
                state.Failures.RemoveAll(item => item < utcNow.AddMinutes(-1));

                if (state.Failures.Count >= 3)
                {
                    state.LockedUntilUtc = utcNow.AddMinutes(2);
                    message = "Too many failed attempts. Account locked for 2 minutes.";
                    return false;
                }

                message = "Invalid credentials.";
                return false;
            }
        }

        public static IEnumerable<object> GetPublicUsers()
        {
            return Users.Select(ToPublicUser).ToList();
        }

        public static object ToPublicUser(DemoUser user)
        {
            return new
            {
                user.Id,
                user.UserName,
                user.Role
            };
        }

        public static IEnumerable<object> GetOrdersForUser(int userId)
        {
            return Orders
                .Where(order => order.UserId == userId)
                .Select(order => new
                {
                    order.Id,
                    order.UserId,
                    order.Item,
                    order.Amount
                })
                .ToList();
        }

        public static IEnumerable<object> VulnerableSqlSearch(string search, out string query)
        {
            var value = search ?? string.Empty;
            query = "SELECT Id, UserName, Role FROM Users WHERE Id = " + value;

            if (value.IndexOf("or 1=1", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("union", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("--", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return GetPublicUsers();
            }

            int id;
            if (!int.TryParse(value, out id))
            {
                return new object[0];
            }

            var user = FindUserById(id);
            return user == null ? new object[0] : new[] { ToPublicUser(user) };
        }

        public static DemoUser VulnerableNoSqlLogin(string rawBody)
        {
            if (string.IsNullOrWhiteSpace(rawBody))
            {
                return null;
            }

            if (rawBody.IndexOf("$gt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rawBody.IndexOf("$ne", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rawBody.IndexOf("$regex", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Users.First();
            }

            return null;
        }

        private static FailedLoginState GetState(string username)
        {
            var key = username ?? string.Empty;
            FailedLoginState state;

            if (!FailedLogins.TryGetValue(key, out state))
            {
                state = new FailedLoginState();
                FailedLogins[key] = state;
            }

            return state;
        }

        private sealed class FailedLoginState
        {
            public List<DateTime> Failures { get; } = new List<DateTime>();

            public DateTime? LockedUntilUtc { get; set; }
        }
    }
}
