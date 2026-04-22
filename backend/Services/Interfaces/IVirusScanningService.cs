namespace backend.Services.Interfaces
{
    /// <summary>
    /// Interface for virus scanning service
    /// This is a placeholder for future integration with antivirus solutions like ClamAV, Windows Defender, or commercial solutions
    /// </summary>
    public interface IVirusScanningService
    {
        /// <summary>
        /// Scans a file for malware and viruses
        /// </summary>
        /// <param name="filePath">Path to the file to scan</param>
        /// <returns>Scan result indicating if the file is safe</returns>
        Task<VirusScanResult> ScanFileAsync(string filePath);

        /// <summary>
        /// Scans file content in memory for malware and viruses
        /// </summary>
        /// <param name="fileContent">File content as byte array</param>
        /// <param name="fileName">Original file name for context</param>
        /// <returns>Scan result indicating if the content is safe</returns>
        Task<VirusScanResult> ScanContentAsync(byte[] fileContent, string fileName);

        /// <summary>
        /// Checks if the virus scanning service is available and operational
        /// </summary>
        /// <returns>True if the service is available, false otherwise</returns>
        Task<bool> IsServiceAvailableAsync();

        /// <summary>
        /// Gets the current virus definition version/date
        /// </summary>
        /// <returns>Information about virus definitions</returns>
        Task<VirusDefinitionInfo> GetVirusDefinitionInfoAsync();

        /// <summary>
        /// Updates virus definitions if supported
        /// </summary>
        /// <returns>True if update was successful, false otherwise</returns>
        Task<bool> UpdateVirusDefinitionsAsync();
    }

    public class VirusScanResult
    {
        public bool IsClean { get; set; }
        public bool ScanCompleted { get; set; }
        public string? ThreatName { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan ScanDuration { get; set; }
        public DateTime ScanTimestamp { get; set; }
        public string ScannerVersion { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalInfo { get; set; } = new();
    }

    public class VirusDefinitionInfo
    {
        public string Version { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public int DefinitionCount { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    public enum ThreatLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum ScanStatus
    {
        NotScanned = 0,
        Scanning = 1,
        Clean = 2,
        Infected = 3,
        Error = 4,
        Quarantined = 5
    }
}