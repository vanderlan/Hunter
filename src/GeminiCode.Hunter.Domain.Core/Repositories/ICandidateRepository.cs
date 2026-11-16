using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using GeminiCode.Hunter.Domain.Core.Filters;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface ICandidateRepository : IBaseEntityRepository<Candidate>
{
    /// <summary>Finds an internal candidate by email (public candidates are kept separate).</summary>
    Task<Candidate?> GetByEmailAsync(string email);
    Task<List<Candidate>> GetRecentCandidatesAsync(int count);
    Task<bool> ExistsByEmailAsync(string email);
    Task<PagedList<Candidate>> ListPaginateAsync(CandidateFilter filter);

    Task<Candidate?> GetByPublicTokenAsync(string publicToken);
    Task<Candidate?> GetByEmailConfirmationTokenAsync(string confirmationToken);

    /// <summary>
    /// Public candidates that never saved their profile before <paramref name="unsavedBefore"/>,
    /// or saved it but never confirmed the email before <paramref name="unconfirmedBefore"/>.
    /// </summary>
    Task<List<Candidate>> GetStalePublicCandidatesAsync(DateTime unsavedBefore, DateTime unconfirmedBefore);
}
