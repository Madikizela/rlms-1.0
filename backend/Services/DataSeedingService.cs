using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Service for seeding initial data into the database
    /// </summary>
    public class DataSeedingService : IDataSeedingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly ILogger<DataSeedingService> _logger;

        // Default system admin credentials
        private const string DEFAULT_ADMIN_EMAIL = "admin@system.local";
        private const string DEFAULT_ADMIN_PASSWORD = "Admin@123!System";
        private const string DEFAULT_ADMIN_FIRST_NAME = "System";
        private const string DEFAULT_ADMIN_LAST_NAME = "Administrator";

        public DataSeedingService(
            ApplicationDbContext context,
            IPasswordHashingService passwordHashingService,
            ILogger<DataSeedingService> logger)
        {
            _context = context;
            _passwordHashingService = passwordHashingService;
            _logger = logger;
        }

        /// <summary>
        /// Seeds the database with initial data including default system admin user
        /// </summary>
        public async Task SeedInitialDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting database seeding process...");

                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();

                // Create default system admin if it doesn't exist
                if (!await DefaultSystemAdminExistsAsync())
                {
                    await CreateDefaultSystemAdminAsync();
                    _logger.LogInformation("Default system admin user created successfully.");
                }
                else
                {
                    _logger.LogInformation("Default system admin user already exists. Skipping creation.");
                }

                _logger.LogInformation("Database seeding process completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during database seeding.");
                throw;
            }
        }

        /// <summary>
        /// Creates the default system admin user if it doesn't exist
        /// </summary>
        public async Task CreateDefaultSystemAdminAsync()
        {
            try
            {
                // Hash the default password
                var hashedPassword = _passwordHashingService.HashPassword(DEFAULT_ADMIN_PASSWORD);

                // Create the default system admin user
                var defaultAdmin = new User
                {
                    FirstName = DEFAULT_ADMIN_FIRST_NAME,
                    LastName = DEFAULT_ADMIN_LAST_NAME,
                    Email = DEFAULT_ADMIN_EMAIL,
                    PasswordHash = hashedPassword,
                    Role = UserRole.SystemAdmin,
                    Status = UserStatus.Active,
                    PhoneNumber = "+1-000-000-0000", // Default system phone
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ClientId = null, // System admin is not associated with any client
                    SkillsDevelopmentProviderId = null, // System admin is not associated with any SDP
                    DepartmentId = null // System admin is not associated with any department
                };

                // Add to database
                _context.Users.Add(defaultAdmin);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Default system admin user created with email: {Email}", DEFAULT_ADMIN_EMAIL);
                _logger.LogWarning("IMPORTANT: Default admin password is '{Password}'. Please change it immediately after first login!", DEFAULT_ADMIN_PASSWORD);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create default system admin user.");
                throw;
            }
        }

        /// <summary>
        /// Checks if the default system admin user exists
        /// </summary>
        public async Task<bool> DefaultSystemAdminExistsAsync()
        {
            try
            {
                return await _context.Users
                    .AnyAsync(u => u.Email == DEFAULT_ADMIN_EMAIL && u.Role == UserRole.SystemAdmin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if default system admin exists.");
                throw;
            }
        }
    }
}