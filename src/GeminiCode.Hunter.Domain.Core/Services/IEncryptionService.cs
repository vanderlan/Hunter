namespace GeminiCode.Hunter.Domain.Core.Services;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
