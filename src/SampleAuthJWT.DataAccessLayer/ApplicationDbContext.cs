namespace SampleAuthJWT.DataAccessLayer;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : AuthenticationDbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}