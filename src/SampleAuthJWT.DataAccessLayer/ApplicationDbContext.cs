using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SampleAuthJWT.DataAccessLayer;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : AuthenticationDbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}