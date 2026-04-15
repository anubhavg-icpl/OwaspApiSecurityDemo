using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace OwaspApiSecurityDemo.App.Infrastructure
{
    public static class DemoTokenService
    {
        private const string Issuer = "owasp-api-demo";
        private const string Audience = "presentation-client";
        private const string SigningKey = "DemoSigningKey-ChangeMe-For-Production";
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        public static string CreateVulnerableToken(DemoUser user)
        {
            var header = new Dictionary<string, object>
            {
                { "alg", "none" },
                { "typ", "JWT" }
            };

            var payload = new Dictionary<string, object>
            {
                { "sub", user.UserName },
                { "userId", user.Id },
                { "role", user.Role },
                { "exp", ToUnixTime(DateTime.UtcNow.AddHours(24)) }
            };

            return Base64UrlEncode(Serializer.Serialize(header)) + "." + Base64UrlEncode(Serializer.Serialize(payload)) + ".";
        }

        public static string CreateSecureToken(DemoUser user, TimeSpan lifetime)
        {
            var header = new Dictionary<string, object>
            {
                { "alg", "HS256" },
                { "typ", "JWT" }
            };

            var payload = new Dictionary<string, object>
            {
                { "sub", user.UserName },
                { "userId", user.Id },
                { "role", user.Role },
                { "iss", Issuer },
                { "aud", Audience },
                { "exp", ToUnixTime(DateTime.UtcNow.Add(lifetime)) }
            };

            var encodedHeader = Base64UrlEncode(Serializer.Serialize(header));
            var encodedPayload = Base64UrlEncode(Serializer.Serialize(payload));
            var signature = ComputeSignature(encodedHeader + "." + encodedPayload);

            return encodedHeader + "." + encodedPayload + "." + signature;
        }

        public static bool TryValidateVulnerableToken(string token, out TokenPrincipal principal, out string error)
        {
            principal = null;
            error = null;

            Dictionary<string, object> payload;
            if (!TryReadPayload(token, out payload, out error))
            {
                return false;
            }

            principal = BuildPrincipal(payload);
            return principal != null;
        }

        public static bool TryValidateSecureToken(string token, out TokenPrincipal principal, out string error)
        {
            principal = null;
            error = null;

            var parts = (token ?? string.Empty).Split('.');
            if (parts.Length != 3)
            {
                error = "Token must contain three JWT segments.";
                return false;
            }

            Dictionary<string, object> header;
            Dictionary<string, object> payload;

            try
            {
                header = Serializer.Deserialize<Dictionary<string, object>>(Base64UrlDecode(parts[0]));
                payload = Serializer.Deserialize<Dictionary<string, object>>(Base64UrlDecode(parts[1]));
            }
            catch (Exception ex)
            {
                error = "Token parsing failed: " + ex.Message;
                return false;
            }

            if (!string.Equals(GetString(header, "alg"), "HS256", StringComparison.Ordinal))
            {
                error = "Only HS256 tokens are allowed.";
                return false;
            }

            var expectedSignature = ComputeSignature(parts[0] + "." + parts[1]);
            if (!FixedTimeEquals(parts[2], expectedSignature))
            {
                error = "Token signature is invalid.";
                return false;
            }

            if (!string.Equals(GetString(payload, "iss"), Issuer, StringComparison.Ordinal) ||
                !string.Equals(GetString(payload, "aud"), Audience, StringComparison.Ordinal))
            {
                error = "Issuer or audience validation failed.";
                return false;
            }

            long expiresAt;
            if (!TryGetInt64(payload, "exp", out expiresAt))
            {
                error = "Token is missing an expiration value.";
                return false;
            }

            var expiresUtc = FromUnixTime(expiresAt);
            if (expiresUtc <= DateTime.UtcNow)
            {
                error = "Token has expired.";
                return false;
            }

            principal = BuildPrincipal(payload);
            if (principal == null)
            {
                error = "Token payload is incomplete.";
                return false;
            }

            principal.ExpiresUtc = expiresUtc;
            return true;
        }

        private static bool TryReadPayload(string token, out Dictionary<string, object> payload, out string error)
        {
            payload = null;
            error = null;

            var parts = (token ?? string.Empty).Split('.');
            if (parts.Length < 2)
            {
                error = "Token must contain at least header and payload segments.";
                return false;
            }

            try
            {
                payload = Serializer.Deserialize<Dictionary<string, object>>(Base64UrlDecode(parts[1]));
                return true;
            }
            catch (Exception ex)
            {
                error = "Token parsing failed: " + ex.Message;
                return false;
            }
        }

        private static TokenPrincipal BuildPrincipal(IDictionary<string, object> payload)
        {
            if (payload == null)
            {
                return null;
            }

            int userId;
            if (!TryGetInt32(payload, "userId", out userId))
            {
                return null;
            }

            var subject = GetString(payload, "sub");
            var role = GetString(payload, "role");

            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(role))
            {
                return null;
            }

            return new TokenPrincipal
            {
                UserId = userId,
                Subject = subject,
                Role = role
            };
        }

        private static string ComputeSignature(string input)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SigningKey)))
            {
                return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(input)));
            }
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left ?? string.Empty);
            var rightBytes = Encoding.UTF8.GetBytes(right ?? string.Empty);

            if (leftBytes.Length != rightBytes.Length)
            {
                return false;
            }

            var mismatch = 0;
            for (var index = 0; index < leftBytes.Length; index++)
            {
                mismatch |= leftBytes[index] ^ rightBytes[index];
            }

            return mismatch == 0;
        }

        private static string Base64UrlEncode(string value)
        {
            return Base64UrlEncode(Encoding.UTF8.GetBytes(value));
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string Base64UrlDecode(string value)
        {
            var padded = (value ?? string.Empty).Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2:
                    padded += "==";
                    break;
                case 3:
                    padded += "=";
                    break;
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        }

        private static string GetString(IDictionary<string, object> payload, string key)
        {
            object value;
            return payload.TryGetValue(key, out value) && value != null ? value.ToString() : null;
        }

        private static bool TryGetInt32(IDictionary<string, object> payload, string key, out int value)
        {
            long rawValue;
            if (TryGetInt64(payload, key, out rawValue) && rawValue <= int.MaxValue && rawValue >= int.MinValue)
            {
                value = (int)rawValue;
                return true;
            }

            value = 0;
            return false;
        }

        private static bool TryGetInt64(IDictionary<string, object> payload, string key, out long value)
        {
            object rawValue;
            if (payload.TryGetValue(key, out rawValue) && rawValue != null)
            {
                return long.TryParse(rawValue.ToString(), out value);
            }

            value = 0;
            return false;
        }

        private static long ToUnixTime(DateTime utcDateTime)
        {
            return (long)(utcDateTime - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        private static DateTime FromUnixTime(long value)
        {
            return new DateTime(1970, 1, 1).AddSeconds(value);
        }
    }
}
