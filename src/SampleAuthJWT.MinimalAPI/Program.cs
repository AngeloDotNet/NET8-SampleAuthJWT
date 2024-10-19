using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinimalHelpers.Routing;
using SampleAuthJWT.DataAccessLayer;
using SampleAuthJWT.DataAccessLayer.Entities;
using SimpleAuthentication;
using TinyHelpers.AspNetCore.Extensions;

namespace SampleAuthJWT.MinimalAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("SqlConnection");

        builder.Services.AddDbContextPool<ApplicationDbContext>(builder =>
        {
            builder.UseSqlServer(connectionString, options =>
            {
                options.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        });
        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.SignIn.RequireConfirmedEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireDigit = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        builder.Services.AddSimpleAuthentication(builder.Configuration);
        builder.Services.AddDataProtection()
            .SetApplicationName(builder.Environment.ApplicationName)
            .PersistKeysToDbContext<ApplicationDbContext>();

        builder.Services.AddScoped(services =>
        {
            var dataProtectionProvider = services.GetRequiredService<IDataProtectionProvider>();

            var dataProtector = dataProtectionProvider.CreateProtector(nameof(ITimeLimitedDataProtector))
                .ToTimeLimitedDataProtector();

            return dataProtector;
        });

        builder.Services.AddDefaultProblemDetails();
        builder.Services.AddDefaultExceptionHandler();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSimpleAuthentication(builder.Configuration);
        });

        var app = builder.Build();

        //await DatabaseExtension.ConfigureDatabaseAsync(app.Services);

        app.UseHttpsRedirection();
        app.UseExceptionHandler();

        app.UseStatusCodePages();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapEndpoints();
        app.Run();
    }
}
