using System;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;

public interface IUnitOfWork: IDisposable
{
    IUserRepository UserRepository { get; }
    IRefreshTokenRepository RefreshTokenRepository { get; }
    IJobPositionRepository JobPositionRepository { get; }
    ICandidateRepository CandidateRepository { get; }
    Task CommitAsync();
    void DiscardChanges();
}
