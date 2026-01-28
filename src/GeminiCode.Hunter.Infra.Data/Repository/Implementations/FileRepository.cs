using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using System.Threading.Tasks;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository.Implementations;

public sealed class FileRepository(DataContext context) : BaseEntityRepository<File>(context), IFileRepository
{
    public async Task<File> GetByS3KeyAsync(string s3Key)
    {
        return await DataContext.Files.AsNoTracking()
            .FirstOrDefaultAsync(f => f.S3Key == s3Key);
    }
}
