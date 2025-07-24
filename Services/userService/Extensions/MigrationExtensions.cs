using Microsoft.EntityFrameworkCore;
using Polly;

namespace userService.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var retry = Policy
                .Handle<Exception>()
                .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(5));

            retry.Execute(() => dbContext.Database.Migrate());
        }

    }
}
