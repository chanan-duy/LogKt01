using LogKt01.Components;
using LogKt01.Data;
using Microsoft.EntityFrameworkCore;

namespace LogKt01;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents();

		builder.Services.AddDbContext<AppDbContext>(options =>
		{
			options.UseSqlite(builder.Configuration.GetConnectionString("AppDbContext") ??
			                  throw new InvalidOperationException("Connection string 'AppDbContext' not found."));
		});

		var app = builder.Build();

		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error");
			app.UseHsts();

			using var scope = app.Services.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			await dbContext.Database.EnsureCreatedAsync();
		}

		app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
		app.UseHttpsRedirection();

		app.UseAntiforgery();

		app.MapStaticAssets();
		app.MapRazorComponents<App>()
			.AddInteractiveServerRenderMode();

		await app.RunAsync();
	}
}
