using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwaspApiSecurityDemo.App.Infrastructure;

namespace OwaspApiSecurityDemo.App.Tests.Infrastructure
{
    [TestClass]
    public sealed class DemoTokenServiceTests
    {
        [TestMethod]
        public void TryValidateVulnerableToken_AcceptsTamperedRoleClaim()
        {
            var alice = DemoStore.FindUserByUserName("alice");
            var originalToken = DemoTokenService.CreateVulnerableToken(alice);
            var tamperedToken = RewriteRole(originalToken, "admin");
            TokenPrincipal principal;
            string error;

            var accepted = DemoTokenService.TryValidateVulnerableToken(tamperedToken, out principal, out error);

            Assert.IsTrue(accepted);
            Assert.IsNull(error);
            Assert.IsNotNull(principal);
            Assert.AreEqual("admin", principal.Role);
            Assert.AreEqual("alice", principal.Subject);
        }

        [TestMethod]
        public void TryValidateSecureToken_RejectsTamperedPayload()
        {
            var alice = DemoStore.FindUserByUserName("alice");
            var secureToken = DemoTokenService.CreateSecureToken(alice, TimeSpan.FromMinutes(15));
            var tamperedToken = RewriteRole(secureToken, "admin");
            TokenPrincipal principal;
            string error;

            var accepted = DemoTokenService.TryValidateSecureToken(tamperedToken, out principal, out error);

            Assert.IsFalse(accepted);
            Assert.IsNull(principal);
            StringAssert.Contains(error, "signature");
        }

        [TestMethod]
        public void CreateSecureToken_RoundTripsThroughSecureValidation()
        {
            var admin = DemoStore.FindUserByUserName("admin");
            var token = DemoTokenService.CreateSecureToken(admin, TimeSpan.FromMinutes(15));
            TokenPrincipal principal;
            string error;

            var accepted = DemoTokenService.TryValidateSecureToken(token, out principal, out error);

            Assert.IsTrue(accepted);
            Assert.IsNull(error);
            Assert.IsNotNull(principal);
            Assert.AreEqual("admin", principal.Role);
            Assert.AreEqual(3, principal.UserId);
            Assert.IsTrue(principal.ExpiresUtc > DateTime.UtcNow);
        }

        private static string RewriteRole(string token, string role)
        {
            var serializer = new JavaScriptSerializer();
            var parts = token.Split('.');
            var payload = serializer.Deserialize<Dictionary<string, object>>(Decode(parts[1]));
            payload["role"] = role;
            parts[1] = Encode(serializer.Serialize(payload));
            return string.Join(".", parts);
        }

        private static string Decode(string value)
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
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

        private static string Encode(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
