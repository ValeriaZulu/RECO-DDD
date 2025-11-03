using System;
using System.Linq;
using RECO.Domain.Entities;

namespace RECO.Domain.Services
{
    /// <summary>
    /// Encapsulates domain logic for reviews: average calculation and policy enforcement.
    /// Pure domain code, no infra/application dependencies (follows constitution).
    /// </summary>
    public class ReviewDomainService
    {
        public double CalculateAverage(Title title)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (title.Reviews == null || title.Reviews.Count == 0) return 0.0;
            return title.Reviews.Average(r => r.Rating);
        }

        public void EnforceUserCanReview(Title title, User user)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (title.Reviews.Exists(r => r.UserId == user.Id))
            {
                throw new InvalidOperationException("User has already reviewed this title");
            }
        }
    }
}
