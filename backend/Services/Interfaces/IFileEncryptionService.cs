using backend.Models;

namespace backend.Services.Interfaces
{
    /// <summary>
    /// Interface for secure file encryption and decryption operations
    /// </summary>
    public interface IFileEncryptionService
    {
        /// <summary>
        /// Encrypts a file and returns the encrypted file path and metadata
        /// </summary>
        /// <param name="filePath">Path to the original file</param>
        /// <param name="encryptionKey">Optional custom encryption key</param>
        /// <returns>Encryption result with encrypted file path and metadata</returns>
        Task<FileEncryptionResult> EncryptFileAsync(string filePath, string? encryptionKey = null);

        /// <summary>
        /// Decrypts a file and returns the decrypted file path
        /// </summary>
        /// <param name="encryptedFilePath">Path to the encrypted file</param>
        /// <param name="encryptionKey">Encryption key used for decryption</param>
        /// <returns>Path to the decrypted file</returns>
        Task<string> DecryptFileAsync(string encryptedFilePath, string encryptionKey);

        /// <summary>
        /// Encrypts file content in memory without saving to disk
        /// </summary>
        /// <param name="fileContent">File content as byte array</param>
        /// <param name="encryptionKey">Optional custom encryption key</param>
        /// <returns>Encrypted content and metadata</returns>
        Task<FileEncryptionResult> EncryptContentAsync(byte[] fileContent, string? encryptionKey = null);

        /// <summary>
        /// Decrypts file content in memory
        /// </summary>
        /// <param name="encryptedContent">Encrypted content as byte array</param>
        /// <param name="encryptionKey">Encryption key used for decryption</param>
        /// <returns>Decrypted content as byte array</returns>
        Task<byte[]> DecryptContentAsync(byte[] encryptedContent, string encryptionKey);

        /// <summary>
        /// Generates a secure encryption key
        /// </summary>
        /// <returns>Base64 encoded encryption key</returns>
        string GenerateEncryptionKey();

        /// <summary>
        /// Calculates file hash for integrity verification
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>SHA-256 hash of the file</returns>
        Task<string> CalculateFileHashAsync(string filePath);

        /// <summary>
        /// Calculates content hash for integrity verification
        /// </summary>
        /// <param name="content">Content as byte array</param>
        /// <returns>SHA-256 hash of the content</returns>
        string CalculateContentHash(byte[] content);

        /// <summary>
        /// Securely deletes a file by overwriting it multiple times
        /// </summary>
        /// <param name="filePath">Path to the file to be securely deleted</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> SecureDeleteFileAsync(string filePath);
    }

    /// <summary>
    /// Result of file encryption operation
    /// </summary>
    public class FileEncryptionResult
    {
        /// <summary>
        /// Path to the encrypted file (if saved to disk)
        /// </summary>
        public string? EncryptedFilePath { get; set; }

        /// <summary>
        /// Encrypted content (if processed in memory)
        /// </summary>
        public byte[]? EncryptedContent { get; set; }

        /// <summary>
        /// Encryption key used for the operation
        /// </summary>
        public string EncryptionKey { get; set; } = string.Empty;

        /// <summary>
        /// Encryption algorithm used
        /// </summary>
        public string Algorithm { get; set; } = string.Empty;

        /// <summary>
        /// Initialization vector used for encryption
        /// </summary>
        public string InitializationVector { get; set; } = string.Empty;

        /// <summary>
        /// Hash of the original file for integrity verification
        /// </summary>
        public string OriginalFileHash { get; set; } = string.Empty;

        /// <summary>
        /// Hash of the encrypted file
        /// </summary>
        public string EncryptedFileHash { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when encryption was performed
        /// </summary>
        public DateTime EncryptedAt { get; set; } = DateTime.UtcNow;
    }
}