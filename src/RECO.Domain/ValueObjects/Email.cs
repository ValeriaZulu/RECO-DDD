using System;

namespace RECO.Domain.ValueObjects
{
    /// <summary>
    /// Email value object - immutable and validates format.
    /// Follows SRP per constitution: single responsibility to represent and validate email.
    /// </summary>
    public sealed class Email
    {
        public string Address { get; }

        public Email(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Email cannot be empty", nameof(address));
            if (!address.Contains("@")) throw new ArgumentException("Email must contain @", nameof(address));
            Address = address;
        }

        public override string ToString() => Address;
    }
}
