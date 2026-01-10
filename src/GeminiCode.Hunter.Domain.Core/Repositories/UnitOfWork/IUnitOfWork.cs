using System;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;

public interface IUnitOfWork: IDisposable
{
    IUserRepository UserRepository { get; }
    IRefreshTokenRepository RefreshTokenRepository { get; }
    IJobPositionRepository JobPositionRepository { get; }
    ICandidateRepository CandidateRepository { get; }
    IFileRepository FileRepository { get; }
    ICandidateFileRepository CandidateFileRepository { get; }
    IHiringProcessRepository HiringProcessRepository { get; }
    IHiringProcessNoteRepository HiringProcessNoteRepository { get; }
    IHiringProcessStatusHistoryRepository HiringProcessStatusHistoryRepository { get; }
    Task CommitAsync();
    void DiscardChanges();
}
