using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ApplicationServices.Contracts;

namespace ApplicationServices.Implementations
{
    public class ImageService : IImageService
    {
        private readonly string _baseImageDirectory;
        private readonly string _bookCoversDirectory;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        public ImageService()
        {
            // Set up directories relative to the application's base directory
            _baseImageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            _bookCoversDirectory = Path.Combine(_baseImageDirectory, "BookCovers");
            
            // Ensure directories exist
            EnsureDirectoriesExist();
        }

        public async Task<string> SaveBookCoverAsync(string sourceImagePath, int bookId)
        {
            try
            {
                if (!ValidateImageFile(sourceImagePath))
                    throw new ArgumentException("Invalid image file format");

                var extension = Path.GetExtension(sourceImagePath).ToLowerInvariant();
                var fileName = $"book_{bookId}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                var destinationPath = Path.Combine(_bookCoversDirectory, fileName);

                // Copy the file to the destination
                await CopyFileAsync(sourceImagePath, destinationPath);

                // Return relative path from the Images directory
                return Path.Combine("BookCovers", fileName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save book cover: {ex.Message}", ex);
            }
        }

        public string GetFullImagePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return GetDefaultCoverPath();

            var fullPath = Path.Combine(_baseImageDirectory, relativePath);
            return File.Exists(fullPath) ? fullPath : GetDefaultCoverPath();
        }

        public async Task<bool> DeleteBookCoverAsync(string relativePath)
        {
            try
            {
                if (string.IsNullOrEmpty(relativePath))
                    return false;

                var fullPath = Path.Combine(_baseImageDirectory, relativePath);
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ImageExists(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return false;

            var fullPath = Path.Combine(_baseImageDirectory, relativePath);
            return File.Exists(fullPath);
        }

        public string GetDefaultCoverPath()
        {
            return Path.Combine(_baseImageDirectory, "book_icon_158035.png");
        }

        public bool ValidateImageFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return Array.Exists(_allowedExtensions, ext => ext == extension);
        }

        public async Task<string> CopyImageToBookCoversDirectoryAsync(string sourceImagePath, int bookId)
        {
            return await SaveBookCoverAsync(sourceImagePath, bookId);
        }

        public async Task<string> DownloadImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                    throw new ArgumentException("Image URL cannot be empty");

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30); // Set timeout for download

                // Download the image
                var response = await httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                // Get the content type to determine file extension
                var contentType = response.Content.Headers.ContentType?.MediaType;
                var extension = GetExtensionFromContentType(contentType) ?? ".jpg"; // Default to .jpg

                // Create a temporary file path
                var tempDirectory = Path.GetTempPath();
                var tempFileName = $"temp_image_{Guid.NewGuid():N}{extension}";
                var tempFilePath = Path.Combine(tempDirectory, tempFileName);

                // Save the downloaded content to the temporary file
                await using var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write);
                await response.Content.CopyToAsync(fileStream);

                // Validate the downloaded image
                if (!ValidateImageFile(tempFilePath))
                {
                    File.Delete(tempFilePath);
                    throw new InvalidOperationException("Downloaded file is not a valid image");
                }

                return tempFilePath;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to download image from URL: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new InvalidOperationException("Image download timed out", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error downloading image: {ex.Message}", ex);
            }
        }

        private void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(_baseImageDirectory))
                Directory.CreateDirectory(_baseImageDirectory);

            if (!Directory.Exists(_bookCoversDirectory))
                Directory.CreateDirectory(_bookCoversDirectory);
        }

        private async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            await sourceStream.CopyToAsync(destinationStream);
        }

        private string GetExtensionFromContentType(string contentType)
        {
            return contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/bmp" => ".bmp",
                _ => null,
            };
        }
    }
}
