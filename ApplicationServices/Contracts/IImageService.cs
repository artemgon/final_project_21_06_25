using System.Threading.Tasks;

namespace ApplicationServices.Contracts
{
    public interface IImageService
    {
        /// <summary>
        /// Saves an image file to the book covers directory and returns the relative path
        /// </summary>
        /// <param name="sourceImagePath">Full path to the source image file</param>
        /// <param name="bookId">ID of the book (used for filename)</param>
        /// <returns>Relative path to the saved image</returns>
        Task<string> SaveBookCoverAsync(string sourceImagePath, int bookId);
        
        /// <summary>
        /// Gets the full path to a book cover image
        /// </summary>
        /// <param name="relativePath">Relative path stored in database</param>
        /// <returns>Full path to the image file</returns>
        string GetFullImagePath(string relativePath);
        
        /// <summary>
        /// Deletes a book cover image file
        /// </summary>
        /// <param name="relativePath">Relative path to the image file</param>
        /// <returns>True if file was deleted successfully</returns>
        Task<bool> DeleteBookCoverAsync(string relativePath);
        
        /// <summary>
        /// Checks if an image file exists
        /// </summary>
        /// <param name="relativePath">Relative path to check</param>
        /// <returns>True if file exists</returns>
        bool ImageExists(string relativePath);
        
        /// <summary>
        /// Gets the default cover image path for books without covers
        /// </summary>
        /// <returns>Path to default cover image</returns>
        string GetDefaultCoverPath();
        
        /// <summary>
        /// Validates if the file is a proper image file
        /// </summary>
        /// <param name="filePath">Full path to the file</param>
        /// <returns>True if the file is a valid image</returns>
        bool ValidateImageFile(string filePath);
        
        /// <summary>
        /// Downloads an image from a URL and saves it to a temporary location
        /// </summary>
        /// <param name="imageUrl">URL of the image to download</param>
        /// <returns>Path to the downloaded temporary file</returns>
        Task<string> DownloadImageAsync(string imageUrl);
    }
}
