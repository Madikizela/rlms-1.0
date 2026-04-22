using backend.Models;

namespace backend.Services
{
    /// <summary>
    /// Interface for data seeding operations
    /// </summary>
    public interface IDataSeedingService
    {
        /// <summary>
        /// Seeds the database with initial data including default system admin user
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task SeedInitialDataAsync();

        /// <summary>
        /// Creates the default system admin user if it doesn't exist
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task CreateDefaultSystemAdminAsync();

        /// <summary>
        /// Checks if the default system admin user exists
        /// </summary>
        /// <returns>True if the default admin exists, false otherwise</returns>
        Task<bool> DefaultSystemAdminExistsAsync();
    }
}