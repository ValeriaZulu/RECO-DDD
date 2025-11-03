using System;

namespace RECO.Domain.ValueObjects
{
    /// <summary>
    /// Username value object - immutable and validates simple rules.
    /// </summary>
    public sealed class Username
    {
        public string Value { get; }

        public Username(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Username cannot be empty", nameof(value));
            if (value.Length < 3) throw new ArgumentException("Username must be at least 3 characters long", nameof(value));
            Value = value;
        }

        public override string ToString() => Value;
    }
}
