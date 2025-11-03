using System;
using System.Collections.Generic;

namespace RECO.Domain.Entities
{
    /// <summary>
    /// Domain entity: User
    /// Follows SRP: this class represents the user aggregate root state only (see constitution).
    /// </summary>
    public class User
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string? DisplayName { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Navigation: each user has one profile
        public Profile? Profile { get; private set; }

        private User() { }

        public User(Guid id, string email, string passwordHash, string? displayName = null)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("email");
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("passwordHash");

            Id = id;
            Email = email;
            PasswordHash = passwordHash;
            DisplayName = displayName;
            CreatedAt = DateTime.UtcNow;
        }

        public void SetProfile(Profile profile)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }
    }
}
