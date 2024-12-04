using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TodoApi;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : IdentityDbContext<ScheduleUser>(options)
{
    public DbSet<Schedule> Schedule => Set<Schedule>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Schedule>()
               .HasOne<ScheduleUser>()
               .WithMany()
               .HasForeignKey(t => t.OwnerId)
               .HasPrincipalKey(u => u.Id);

        base.OnModelCreating(builder);
    }
}
