using System;
using FluentAssertions;
using RECO.Domain.Entities;
using Xunit;

namespace RECO.Domain.Tests
{
    public class UserTests
    {
        [Fact]
        public void CreateUser_WithValidData_SetsProperties()
        {
            var id = Guid.NewGuid();
            var user = new User(id, "test@example.com", "hashedpwd", "Tester");

            user.Id.Should().Be(id);
            user.Email.Should().Be("test@example.com");
            user.DisplayName.Should().Be("Tester");
        }

        [Fact]
        public void CreateUser_WithEmptyEmail_Throws()
        {
            Action act = () => new User(Guid.NewGuid(), "", "pwd");
            act.Should().Throw<ArgumentException>();
        }
    }
}
