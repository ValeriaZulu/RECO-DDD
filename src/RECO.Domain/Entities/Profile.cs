using System;
using System.Collections.Generic;

namespace RECO.Domain.Entities
{
    public class Profile
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Simple preference collections - normalized in persistence
        public List<GenrePreference> GenrePreferences { get; private set; } = new();
        public List<PersonPreference> PersonPreferences { get; private set; } = new();

        private Profile() { }

        public Profile(Guid id, Guid userId)
        {
            Id = id;
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
        }

        public void AddGenrePreference(GenrePreference pref)
        {
            if (pref == null) throw new ArgumentNullException(nameof(pref));
            GenrePreferences.Add(pref);
        }

        public void AddPersonPreference(PersonPreference pref)
        {
            if (pref == null) throw new ArgumentNullException(nameof(pref));
            PersonPreferences.Add(pref);
        }
    }

    public class GenrePreference
    {
        public int GenreId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        private GenrePreference() { }
        public GenrePreference(int genreId, string name)
        {
            GenreId = genreId;
            Name = name;
        }
    }

    public class PersonPreference
    {
        public int PersonId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        private PersonPreference() { }
        public PersonPreference(int personId, string name)
        {
            PersonId = personId;
            Name = name;
        }
    }
}
