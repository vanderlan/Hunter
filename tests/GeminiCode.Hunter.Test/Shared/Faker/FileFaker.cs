using System;
using Bogus;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;

namespace GeminiCode.Hunter.Test.Shared.Faker;

public static class FileFaker
{
    public static File Get()
    {
        return new Faker<File>()
             .RuleFor(o => o.FileId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.PublicId, f => Guid.NewGuid().ToString())
             .RuleFor(o => o.FileName, f => f.System.FileName("pdf"))
             .RuleFor(o => o.ContentType, f => "application/pdf")
             .RuleFor(o => o.FileSize, f => f.Random.Long(1024, 1024 * 1024 * 10))
             .RuleFor(o => o.StorageType, f => StorageType.Database)
             .RuleFor(o => o.FileData, f => f.Random.Bytes(1024))
             .RuleFor(o => o.S3Key, f => null)
             .RuleFor(o => o.S3Bucket, f => null)
             .Generate();
    }
}
