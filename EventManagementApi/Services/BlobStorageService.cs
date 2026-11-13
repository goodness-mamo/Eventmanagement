using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace EventManagementApi.Services;

public sealed class BlobStorageService
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    private readonly BlobServiceClient _blobServiceClient;

    private static readonly Dictionary<string, string> AllowedImages =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private static readonly HashSet<string> AllowedContainers =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "event-banners",
            "profile-pictures"
        };

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    // UPLOAD IMAGE
    public async Task<string> UploadImageAsync(
        IFormFile file,
        string containerName,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Please select an image.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException(
                "Image size cannot exceed 5 MB.");

        if (!AllowedContainers.Contains(containerName))
            throw new ArgumentException(
                "Invalid storage container.");

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedImages.TryGetValue(
            extension, out var expectedContentType))
        {
            throw new ArgumentException(
                "Only JPG, PNG and WEBP images are allowed.");
        }

        if (!string.Equals(
            file.ContentType,
            expectedContentType,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The image content type does not match its file extension.");
        }

        var containerClient =
            _blobServiceClient.GetBlobContainerClient(containerName);

        await containerClient.CreateIfNotExistsAsync(
            PublicAccessType.None,
            cancellationToken: cancellationToken);

        // Generate a unique filename.
        var blobName =
            $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        var blobClient =
            containerClient.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = expectedContentType
                }
            },
            cancellationToken);

        return blobName;
    }

    // DOWNLOAD IMAGE
    public async Task<BlobImage?> DownloadImageAsync(
        string blobName,
        string containerName,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedContainers.Contains(containerName))
            throw new ArgumentException(
                "Invalid storage container.");

        if (string.IsNullOrWhiteSpace(blobName) ||
            Path.GetFileName(blobName) != blobName)
        {
            throw new ArgumentException(
                "Invalid image name.");
        }

        var containerClient =
            _blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        var exists = await blobClient.ExistsAsync(
            cancellationToken);

        if (!exists.Value)
            return null;

        var response = await blobClient.DownloadStreamingAsync(
            cancellationToken: cancellationToken);

        return new BlobImage(
            response.Value.Content,
            response.Value.Details.ContentType ??
            "application/octet-stream");
    }

    // DELETE IMAGE
    public async Task DeleteImageAsync(
        string blobName,
        string containerName,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedContainers.Contains(containerName))
            throw new ArgumentException(
                "Invalid storage container.");

        if (string.IsNullOrWhiteSpace(blobName) ||
            Path.GetFileName(blobName) != blobName)
        {
            throw new ArgumentException(
                "Invalid image name.");
        }

        var containerClient =
            _blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(
            cancellationToken: cancellationToken);
    }
}

// IMAGE RESPONSE MODEL
public sealed record BlobImage(
    Stream Content,
    string ContentType);