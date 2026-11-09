using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

public interface IBlobStorageService
{
    Task<Uri> UploadAsync(IFormFile file, string containerName, CancellationToken cancellationToken = default);
}

public sealed class BlobStorageService : IBlobStorageService
{
    private readonly IConfiguration _configuration;
    public BlobStorageService(IConfiguration configuration) => _configuration = configuration;

    public async Task<Uri> UploadAsync(IFormFile file, string containerName, CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration["AzureBlobStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Azure Blob Storage is not configured. Set AzureBlobStorage__ConnectionString.");

        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only JPEG, PNG and WebP images are accepted.");
        if (file.Length == 0 || file.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("The image must be between 1 byte and 5 MB.");

        var service = new BlobServiceClient(connectionString);
        var container = service.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
        var extension = Path.GetExtension(file.FileName);
        var blob = container.GetBlobClient($"{Guid.NewGuid():N}{extension}");
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType }, cancellationToken: cancellationToken);
        return blob.Uri;
    }
}
