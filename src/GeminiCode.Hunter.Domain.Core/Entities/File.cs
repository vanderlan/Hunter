using GeminiCode.Hunter.Domain.Core.Entities.Enuns;

namespace GeminiCode.Hunter.Domain.Core.Entities;

public class File : BaseEntity
{
    public long FileId { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSize { get; set; }
    public StorageType StorageType { get; set; }
    public byte[]? FileData { get; set; }
    public string? S3Key { get; set; }
    public string? S3Bucket { get; set; }
}
