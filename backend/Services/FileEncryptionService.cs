using System.Security.Cryptography;
using System.Text;
using backend.Services.Interfaces;

namespace backend.Services
{
    /// <summary>
    /// Implementation of secure file encryption service using AES-256 encryption
    /// </summary>
    public class FileEncryptionService : IFileEncryptionService
    {
        private readonly ILogger<FileEncryptionService> _logger;
        private const string ENCRYPTION_ALGORITHM = "AES-256-CBC";
        private const int KEY_SIZE = 256; // AES-256
        private const int BLOCK_SIZE = 128; // AES block size

        public FileEncryptionService(ILogger<FileEncryptionService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Encrypts a file and saves the encrypted version
        /// </summary>
        public async Task<FileEncryptionResult> EncryptFileAsync(string filePath, string? encryptionKey = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                // Generate encryption key if not provided
                var key = encryptionKey ?? GenerateEncryptionKey();
                
                // Read original file
                var originalContent = await File.ReadAllBytesAsync(filePath);
                var originalHash = CalculateContentHash(originalContent);

                // Encrypt content
                var encryptionResult = await EncryptContentAsync(originalContent, key);
                
                // Generate encrypted file path
                var encryptedFilePath = GenerateEncryptedFilePath(filePath);
                
                // Save encrypted content to file
                if (encryptionResult.EncryptedContent != null)
                {
                    await File.WriteAllBytesAsync(encryptedFilePath, encryptionResult.EncryptedContent);
                }

                // Calculate encrypted file hash
                var encryptedHash = await CalculateFileHashAsync(encryptedFilePath);

                _logger.LogInformation("File encrypted successfully: {OriginalPath} -> {EncryptedPath}", 
                    filePath, encryptedFilePath);

                return new FileEncryptionResult
                {
                    EncryptedFilePath = encryptedFilePath,
                    EncryptionKey = key,
                    Algorithm = ENCRYPTION_ALGORITHM,
                    InitializationVector = encryptionResult.InitializationVector,
                    OriginalFileHash = originalHash,
                    EncryptedFileHash = encryptedHash,
                    EncryptedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting file: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Decrypts a file and returns the decrypted file path
        /// </summary>
        public async Task<string> DecryptFileAsync(string encryptedFilePath, string encryptionKey)
        {
            try
            {
                if (!File.Exists(encryptedFilePath))
                {
                    throw new FileNotFoundException($"Encrypted file not found: {encryptedFilePath}");
                }

                // Read encrypted content
                var encryptedContent = await File.ReadAllBytesAsync(encryptedFilePath);
                
                // Decrypt content
                var decryptedContent = await DecryptContentAsync(encryptedContent, encryptionKey);
                
                // Generate decrypted file path
                var decryptedFilePath = GenerateDecryptedFilePath(encryptedFilePath);
                
                // Save decrypted content
                await File.WriteAllBytesAsync(decryptedFilePath, decryptedContent);

                _logger.LogInformation("File decrypted successfully: {EncryptedPath} -> {DecryptedPath}", 
                    encryptedFilePath, decryptedFilePath);

                return decryptedFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting file: {FilePath}", encryptedFilePath);
                throw;
            }
        }

        /// <summary>
        /// Encrypts content in memory using AES-256
        /// </summary>
        public async Task<FileEncryptionResult> EncryptContentAsync(byte[] fileContent, string? encryptionKey = null)
        {
            try
            {
                var key = encryptionKey ?? GenerateEncryptionKey();
                var keyBytes = Convert.FromBase64String(key);

                using var aes = Aes.Create();
                aes.KeySize = KEY_SIZE;
                aes.BlockSize = BLOCK_SIZE;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = keyBytes;
                aes.GenerateIV();

                var iv = Convert.ToBase64String(aes.IV);

                using var encryptor = aes.CreateEncryptor();
                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                
                await csEncrypt.WriteAsync(fileContent, 0, fileContent.Length);
                await csEncrypt.FlushFinalBlockAsync();
                
                var encryptedContent = msEncrypt.ToArray();
                var originalHash = CalculateContentHash(fileContent);
                var encryptedHash = CalculateContentHash(encryptedContent);

                _logger.LogInformation("Content encrypted successfully. Original size: {OriginalSize}, Encrypted size: {EncryptedSize}", 
                    fileContent.Length, encryptedContent.Length);

                return new FileEncryptionResult
                {
                    EncryptedContent = encryptedContent,
                    EncryptionKey = key,
                    Algorithm = ENCRYPTION_ALGORITHM,
                    InitializationVector = iv,
                    OriginalFileHash = originalHash,
                    EncryptedFileHash = encryptedHash,
                    EncryptedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting content");
                throw;
            }
        }

        /// <summary>
        /// Decrypts content in memory using AES-256
        /// </summary>
        public async Task<byte[]> DecryptContentAsync(byte[] encryptedContent, string encryptionKey)
        {
            try
            {
                var keyBytes = Convert.FromBase64String(encryptionKey);

                using var aes = Aes.Create();
                aes.KeySize = KEY_SIZE;
                aes.BlockSize = BLOCK_SIZE;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = keyBytes;

                // Extract IV from the beginning of encrypted content
                var iv = new byte[aes.BlockSize / 8];
                Array.Copy(encryptedContent, 0, iv, 0, iv.Length);
                aes.IV = iv;

                // Get actual encrypted data (without IV)
                var actualEncryptedData = new byte[encryptedContent.Length - iv.Length];
                Array.Copy(encryptedContent, iv.Length, actualEncryptedData, 0, actualEncryptedData.Length);

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(actualEncryptedData);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var msResult = new MemoryStream();
                
                await csDecrypt.CopyToAsync(msResult);
                var decryptedContent = msResult.ToArray();

                _logger.LogInformation("Content decrypted successfully. Encrypted size: {EncryptedSize}, Decrypted size: {DecryptedSize}", 
                    encryptedContent.Length, decryptedContent.Length);

                return decryptedContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting content");
                throw;
            }
        }

        /// <summary>
        /// Generates a secure 256-bit encryption key
        /// </summary>
        public string GenerateEncryptionKey()
        {
            using var rng = RandomNumberGenerator.Create();
            var keyBytes = new byte[32]; // 256 bits
            rng.GetBytes(keyBytes);
            return Convert.ToBase64String(keyBytes);
        }

        /// <summary>
        /// Calculates SHA-256 hash of a file
        /// </summary>
        public async Task<string> CalculateFileHashAsync(string filePath)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hashBytes = await sha256.ComputeHashAsync(stream);
                return Convert.ToBase64String(hashBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating file hash: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Calculates SHA-256 hash of content
        /// </summary>
        public string CalculateContentHash(byte[] content)
        {
            try
            {
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(content);
                return Convert.ToBase64String(hashBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating content hash");
                throw;
            }
        }

        /// <summary>
        /// Securely deletes a file by overwriting it multiple times
        /// </summary>
        public async Task<bool> SecureDeleteFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found for secure deletion: {FilePath}", filePath);
                    return false;
                }

                var fileInfo = new FileInfo(filePath);
                var fileSize = fileInfo.Length;

                // Overwrite file multiple times with random data
                using var rng = RandomNumberGenerator.Create();
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Write);

                for (int pass = 0; pass < 3; pass++)
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                    
                    var buffer = new byte[4096];
                    long bytesWritten = 0;
                    
                    while (bytesWritten < fileSize)
                    {
                        var bytesToWrite = (int)Math.Min(buffer.Length, fileSize - bytesWritten);
                        rng.GetBytes(buffer, 0, bytesToWrite);
                        await fileStream.WriteAsync(buffer, 0, bytesToWrite);
                        bytesWritten += bytesToWrite;
                    }
                    
                    await fileStream.FlushAsync();
                }

                fileStream.Close();

                // Delete the file
                File.Delete(filePath);

                _logger.LogInformation("File securely deleted: {FilePath}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error securely deleting file: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Generates encrypted file path
        /// </summary>
        private string GenerateEncryptedFilePath(string originalPath)
        {
            var directory = Path.GetDirectoryName(originalPath) ?? "";
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalPath);
            var extension = Path.GetExtension(originalPath);
            
            return Path.Combine(directory, $"{fileNameWithoutExtension}_encrypted{extension}.enc");
        }

        /// <summary>
        /// Generates decrypted file path
        /// </summary>
        private string GenerateDecryptedFilePath(string encryptedPath)
        {
            var directory = Path.GetDirectoryName(encryptedPath) ?? "";
            var fileName = Path.GetFileNameWithoutExtension(encryptedPath);
            
            // Remove "_encrypted" suffix and ".enc" extension
            if (fileName.EndsWith("_encrypted"))
            {
                fileName = fileName.Substring(0, fileName.Length - 10);
            }
            
            return Path.Combine(directory, $"{fileName}_decrypted.tmp");
        }
    }
}