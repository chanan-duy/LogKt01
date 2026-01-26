using Microsoft.EntityFrameworkCore;

namespace LogKt01.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	public DbSet<TaskEntity> Tasks { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<TaskEntity>()
			.Property(x => x.Id)
			.ValueGeneratedOnAdd();
	}
}
