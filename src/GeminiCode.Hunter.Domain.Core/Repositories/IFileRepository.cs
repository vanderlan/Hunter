using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface IFileRepository : IBaseEntityRepository<File>
{
    Task<File> GetByS3KeyAsync(string s3Key);
}
