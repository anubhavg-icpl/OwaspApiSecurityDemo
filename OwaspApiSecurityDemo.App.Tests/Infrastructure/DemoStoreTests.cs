using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwaspApiSecurityDemo.App.Infrastructure;

namespace OwaspApiSecurityDemo.App.Tests.Infrastructure
{
    [TestClass]
    public sealed class DemoStoreTests
    {
        [TestInitialize]
        public void Initialize()
        {
            DemoStore.ResetFailedLoginTracking();
        }

        [TestMethod]
        public void ValidateCredentials_ReturnsKnownUser()
        {
            DemoUser user;

            var valid = DemoStore.ValidateCredentials("alice", "alice123", out user);

            Assert.IsTrue(valid);
            Assert.IsNotNull(user);
            Assert.AreEqual("alice", user.UserName);
            Assert.AreEqual("user", user.Role);
        }

        [TestMethod]
        public void TryAuthenticateSecure_LocksAccountAfterThreeFailures_ThenAllowsLoginAfterExpiry()
        {
            DemoUser user;
            string message;
            var now = new DateTime(2026, 4, 17, 0, 0, 0, DateTimeKind.Utc);

            Assert.IsFalse(DemoStore.TryAuthenticateSecure("alice", "wrong", now, out user, out message));
            Assert.AreEqual("Invalid credentials.", message);

            Assert.IsFalse(DemoStore.TryAuthenticateSecure("alice", "wrong", now.AddSeconds(10), out user, out message));
            Assert.AreEqual("Invalid credentials.", message);

            Assert.IsFalse(DemoStore.TryAuthenticateSecure("alice", "wrong", now.AddSeconds(20), out user, out message));
            Assert.AreEqual("Too many failed attempts. Account locked for 2 minutes.", message);

            Assert.IsFalse(DemoStore.TryAuthenticateSecure("alice", "alice123", now.AddSeconds(30), out user, out message));
            Assert.AreEqual("Account is temporarily locked because of repeated failed login attempts.", message);

            Assert.IsTrue(DemoStore.TryAuthenticateSecure("alice", "alice123", now.AddMinutes(3), out user, out message));
            Assert.IsNotNull(user);
            Assert.AreEqual("alice", user.UserName);
            Assert.IsNull(message);
        }

        [TestMethod]
        public void VulnerableSqlSearch_InjectionPayloadReturnsAllUsers()
        {
            string query;

            var users = DemoStore.VulnerableSqlSearch("1 OR 1=1--", out query);

            Assert.AreEqual("SELECT Id, UserName, Role FROM Users WHERE Id = 1 OR 1=1--", query);
            Assert.AreEqual(3, CountEnumerable(users));
        }

        [TestMethod]
        public void VulnerableNoSqlLogin_OperatorPayloadBypassesCredentialChecks()
        {
            var user = DemoStore.VulnerableNoSqlLogin("{\"username\":{\"$gt\":\"\"},\"password\":{\"$gt\":\"\"}}");

            Assert.IsNotNull(user);
            Assert.AreEqual("alice", user.UserName);
        }

        private static int CountEnumerable(System.Collections.IEnumerable values)
        {
            var count = 0;
            foreach (var value in values)
            {
                count++;
            }

            return count;
        }
    }
}
